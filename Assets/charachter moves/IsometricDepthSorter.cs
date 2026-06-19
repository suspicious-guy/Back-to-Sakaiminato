using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsometricDepthSorter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer playerRenderer;

    [Header("Углы передней грани (локальные координаты)")]
    [Tooltip("Левый нижний угол передней стены в локальных координатах объекта.")]
    [SerializeField] private Vector2 frontCornerLeft = new Vector2(-0.5f, 0f);

    [Tooltip("Правый нижний угол передней стены в локальных координатах объекта.")]
    [SerializeField] private Vector2 frontCornerRight = new Vector2(0.5f, -0.25f);

    [Header("Непрерывная сортировка")]
    [Tooltip("Рекомендуется для сцен с множеством объектов разного размера.")]
    [SerializeField] private bool useContinuousSorting = false;
    [SerializeField] private float sortingMultiplier = 10f;

    [Header("Бинарный режим (если useContinuousSorting = false)")]
    [SerializeField] private int orderWhenInFront = 2;
    [SerializeField] private int orderWhenBehind = 0;
    [SerializeField] private int playerOrderBehind = 1;
    [SerializeField] private int playerOrderInFront = 3;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerTransform == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerTransform = playerGO.transform;
                playerRenderer = playerGO.GetComponent<SpriteRenderer>();
            }
            else
            {
                Debug.LogWarning($"[IsometricDepthSorter] '{gameObject.name}': " +
                                 "игрок не найден. Задай playerTransform в инспекторе " +
                                 "или добавь тег 'Player' на объект игрока.");
            }
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;
        if (useContinuousSorting) UpdateContinuous();
        else UpdateBinary();
    }

    /// <summary>
    /// Возвращает Y диагональной линии передней грани в точке X игрока.
    /// Линия проходит через два угла — левый и правый — передней стены объекта.
    /// Для X игрока интерполируем Y вдоль этой диагонали.
    /// </summary>
    private float GetThresholdY(float playerWorldX)
    {
        Vector2 wLeft = (Vector2)transform.position + frontCornerLeft;
        Vector2 wRight = (Vector2)transform.position + frontCornerRight;

        float dx = wRight.x - wLeft.x;
        if (Mathf.Abs(dx) < 0.001f)
            return (wLeft.y + wRight.y) * 0.5f;

        float t = Mathf.InverseLerp(wLeft.x, wRight.x, playerWorldX);
        return Mathf.Lerp(wLeft.y, wRight.y, t);
    }

    private float GetPlayerY()
    {
        if (playerRenderer != null) return playerRenderer.bounds.min.y;
        return playerTransform.position.y;
    }

    private void UpdateBinary()
    {
        float threshold = GetThresholdY(playerTransform.position.x);
        float playerY = GetPlayerY();

        if (playerY < threshold)
        {
            spriteRenderer.sortingOrder = orderWhenInFront;
            if (playerRenderer != null) playerRenderer.sortingOrder = playerOrderBehind;
        }
        else
        {
            spriteRenderer.sortingOrder = orderWhenBehind;
            if (playerRenderer != null) playerRenderer.sortingOrder = playerOrderInFront;
        }
    }

    private void UpdateContinuous()
    {
        float threshold = GetThresholdY(playerTransform.position.x);
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-threshold * sortingMultiplier);

        if (playerRenderer != null)
            playerRenderer.sortingOrder = Mathf.RoundToInt(-GetPlayerY() * sortingMultiplier);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 wLeft  = transform.position + (Vector3)(Vector2)frontCornerLeft;
        Vector3 wRight = transform.position + (Vector3)(Vector2)frontCornerRight;

        // Жёлтая линия — диагональ передней грани
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wLeft, wRight);
        Gizmos.DrawWireSphere(wLeft,  0.07f);
        Gizmos.DrawWireSphere(wRight, 0.07f);

        // Голубая сфера — точка сравнения для текущей позиции игрока
        if (playerTransform != null)
        {
            float t   = Mathf.Clamp01(Mathf.InverseLerp(wLeft.x, wRight.x, playerTransform.position.x));
            Vector3 p = Vector3.Lerp(wLeft, wRight, t);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, 0.1f);
            Gizmos.DrawLine(p, playerTransform.position);
        }
    }
#endif

    public void SetPlayer(GameObject player)
    {
        if (player == null) return;
        playerTransform = player.transform;
        playerRenderer = player.GetComponent<SpriteRenderer>();
    }
}