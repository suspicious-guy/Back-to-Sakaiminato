using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Движение по маршруту")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitAtPointTime = 1f;

    [Header("Обнаружение игрока")]
    public float visionRadius = 3f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Визуализация")]
    public Color visionColor = new Color(1f, 0f, 0f, 0.3f);
    public GameObject visionVisualizer;
    public bool showVisionRadiusInGame = true;

    private int currentWaypoint = 0;
    private bool isWaiting = false;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private bool facingRight = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (visionVisualizer != null && showVisionRadiusInGame)
        {
            UpdateVisionVisualizer();
        }
        else if (visionVisualizer != null && !showVisionRadiusInGame)
        {
            visionVisualizer.SetActive(false);
        }
    }

    void Update()
    {
        MoveAlongPath();
        FlipSprite();

        if (player != null && IsPlayerInVision())
        {
            DetectPlayer();
        }
    }

    void MoveAlongPath()
    {
        if (waypoints.Length == 0) return;
        if (isWaiting) return;

        Transform target = waypoints[currentWaypoint];
        moveDirection = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            StartCoroutine(WaitAtPoint());
        }
    }

    IEnumerator WaitAtPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitAtPointTime);
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        isWaiting = false;
    }

    void FlipSprite()
    {
        if (spriteRenderer == null) return;

        if (moveDirection.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection.x < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    bool IsPlayerInVision()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > visionRadius) return false;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
        if (hit.collider != null) return false;

        return true;
    }

    void DetectPlayer()
    {
        PlayerStealth stealth = player.GetComponent<PlayerStealth>();
        if (stealth != null && stealth.isHidden)
        {
            Debug.Log($"[{gameObject.name}] 🌿 Игрок присел в траве! Не вижу.");
            return;
        }

        Debug.Log($"[{gameObject.name}] ⚠️ Игрок обнаружен!");
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        if (player != null)
        {
            RespawnPoint respawn = FindObjectOfType<RespawnPoint>();
            if (respawn != null)
            {
                player.position = respawn.transform.position;
                Debug.Log($"[{gameObject.name}] Игрок перемещён на точку возрождения");
            }
            else
            {
                Debug.LogWarning("[EnemyController] Нет RespawnPoint на сцене!");
            }
        }
    }

    void UpdateVisionVisualizer()
    {
        if (visionVisualizer != null)
        {
            visionVisualizer.transform.localScale = Vector3.one * visionRadius * 2f;
            SpriteRenderer visRenderer = visionVisualizer.GetComponent<SpriteRenderer>();
            if (visRenderer != null)
            {
                visRenderer.color = visionColor;
                visRenderer.sortingOrder = -1;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = visionColor;
        Gizmos.DrawWireSphere(transform.position, visionRadius);

        if (waypoints != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                }
            }

            if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }
    }
}