using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Движение по маршруту")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float waitAtPointTime = 1f;

    [Header("Обнаружение игрока (овальная зона)")]
    public float visionWidth = 5f;
    public float visionHeight = 4f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Визуализация (отдельные параметры)")]
    public GameObject visionVisualizer;
    public float visualizerWidth = 8f;    // Отдельная ширина для визуализации
    public float visualizerHeight = 6f;   // Отдельная высота для визуализации
    public Color visionColor = new Color(1f, 0f, 0f, 0.3f);
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
            UpdateVisualizerSize();
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
        if (player == null) return false;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;

        float deltaX = playerPos.x - enemyPos.x;
        float deltaY = playerPos.y - enemyPos.y;

        float a = visionWidth / 2f;
        float b = visionHeight / 2f;

        float ellipseValue = (deltaX * deltaX) / (a * a) + (deltaY * deltaY) / (b * b);

        if (ellipseValue > 1f) return false;

        RaycastHit2D hit = Physics2D.Linecast(enemyPos, playerPos, obstacleLayer);
        if (hit.collider != null && !hit.collider.CompareTag("Player")) return false;

        return true;
    }

    void DetectPlayer()
    {
        PlayerStealth stealth = player.GetComponent<PlayerStealth>();
        if (stealth != null && stealth.isHidden)
        {
            return;
        }

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
            }
        }
    }

    void UpdateVisualizerSize()
    {
        if (visionVisualizer == null) return;

        // Используем ОТДЕЛЬНЫЕ параметры для визуализации
        visionVisualizer.transform.localScale = new Vector3(visualizerWidth, visualizerHeight, 1f);

        SpriteRenderer visRenderer = visionVisualizer.GetComponent<SpriteRenderer>();
        if (visRenderer != null)
        {
            visRenderer.color = visionColor;
            visRenderer.sortingOrder = -1;
        }
    }

    // Метод для обновления визуализатора из инспектора
    void OnValidate()
    {
        if (visionVisualizer != null && !Application.isPlaying)
        {
            visionVisualizer.transform.localScale = new Vector3(visualizerWidth, visualizerHeight, 1f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем реальную зону обнаружения (для отладки)
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;

        Vector3 prevPoint = center + new Vector3(visionWidth / 2f, 0, 0);
        for (int i = 1; i <= 360; i++)
        {
            float angle = i * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * visionWidth / 2f;
            float y = Mathf.Sin(angle) * visionHeight / 2f;
            Vector3 newPoint = center + new Vector3(x, y, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }

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
        }
    }
}