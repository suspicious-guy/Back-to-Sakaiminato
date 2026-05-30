using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsometricDepthSorter : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer playerRenderer;

    [Header("Режим линии сравнения")]
    [Tooltip("Horizontal — горизонтальная линия (подходит для маленьких объектов).\n" +
             "Diagonal  — диагональная линия через два угла передней грани (для зданий, навесов).\n" +
             "Manual    — пивот + ручное смещение.")]
    [SerializeField] private DepthMode depthMode = DepthMode.Diagonal;

    public enum DepthMode { Horizontal, Diagonal, Manual }

    [Header("Diagonal — углы передней грани")]
    [Tooltip("Левый нижний угол передней стены объекта в ЛОКАЛЬНЫХ координатах.\n" +
             "Выбери точку в Scene и запиши (x, y) относительно пивота объекта.")]
    [SerializeField] private Vector2 frontCornerLeft = new Vector2(-0.5f, 0f);

    [Tooltip("Правый нижний угол передней стены объекта в ЛОКАЛЬНЫХ координатах.")]
    [SerializeField] private Vector2 frontCornerRight = new Vector2(0.5f, -0.25f);

    [Header("Horizontal — смещение от нижнего края спрайта")]
    [SerializeField] private float depthYOffset = 0f;
    [SerializeField] private bool offsetIsRelative = true;

    [Header("Manual — смещение от пивота")]
    [SerializeField] private Vector2 objectPivotOffset = Vector2.zero;
    [SerializeField] private Vector2 playerPivotOffset = Vector2.zero;

    [Header("Order in Layer (бинарный режим)")]
    [SerializeField] private int orderWhenInFront = 2;
    [SerializeField] private int orderWhenBehind = 0;
    [SerializeField] private int playerOrderBehind = 1;
    [SerializeField] private int playerOrderInFront = 3;

    [Header("Непрерывная сортировка")]
    [Tooltip("Рекомендуется для сцен с множеством объектов.")]
    [SerializeField] private bool useContinuousSorting = false;
    [SerializeField] private float sortingMultiplier = 10f;

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
                Debug.LogWarning($"[IsometricDepthSorter] '{gameObject.name}': игрок не найден.");
            }
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;
        if (useContinuousSorting) UpdateContinuous();
        else UpdateBinary();
    }

    // ─── Ключевой метод: Y линии сравнения в точке X игрока ──────────────────

    /// <summary>
    /// Возвращает Y "порога глубины" объекта для конкретного X игрока.
    ///
    /// В диагональном режиме линия проходит через два угла передней грани.
    /// Для любого X мы интерполируем Y вдоль этой диагонали — именно поэтому
    /// игрок слева и справа оцениваются корректно даже у широких объектов.
    /// </summary>
    private float GetObjectDepthYAtPlayerX(float playerWorldX)
    {
        switch (depthMode)
        {
            case DepthMode.Diagonal:
                {
                    // Переводим локальные углы в мировые координаты
                    Vector2 worldLeft = (Vector2)transform.position + frontCornerLeft;
                    Vector2 worldRight = (Vector2)transform.position + frontCornerRight;

                    float dx = worldRight.x - worldLeft.x;
                    if (Mathf.Abs(dx) < 0.001f)
                        return (worldLeft.y + worldRight.y) * 0.5f;

                    // Линейная интерполяция: находим Y на диагонали при X игрока
                    float t = Mathf.InverseLerp(worldLeft.x, worldRight.x, playerWorldX);
                    return Mathf.Lerp(worldLeft.y, worldRight.y, t);
                }

            case DepthMode.Horizontal:
                {
                    if (spriteRenderer != null)
                    {
                        float minY = spriteRenderer.bounds.min.y;
                        float height = spriteRenderer.bounds.size.y;
                        float offset = offsetIsRelative ? depthYOffset * height : depthYOffset;
                        return minY + offset;
                    }
                    return transform.position.y;
                }

            default: // Manual
                return transform.position.y + objectPivotOffset.y;
        }
    }

    private float GetPlayerDepthY()
    {
        if (playerRenderer != null) return playerRenderer.bounds.min.y;
        return playerTransform.position.y + playerPivotOffset.y;
    }

    // ─── Бинарный режим ───────────────────────────────────────────────────────

    private void UpdateBinary()
    {
        float threshold = GetObjectDepthYAtPlayerX(playerTransform.position.x);
        float playerY = GetPlayerDepthY();

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

    // ─── Непрерывный режим ────────────────────────────────────────────────────

    private void UpdateContinuous()
    {
        float threshold = GetObjectDepthYAtPlayerX(playerTransform.position.x);
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-threshold * sortingMultiplier);

        if (playerRenderer != null)
            playerRenderer.sortingOrder = Mathf.RoundToInt(-GetPlayerDepthY() * sortingMultiplier);
    }

    // ─── Гизмо ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        switch (depthMode)
        {
            case DepthMode.Diagonal:
            {
                Vector3 wLeft  = transform.position + (Vector3)(Vector2)frontCornerLeft;
                Vector3 wRight = transform.position + (Vector3)(Vector2)frontCornerRight;

                // Диагональная линия сравнения — жёлтая
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(wLeft, wRight);
                Gizmos.DrawWireSphere(wLeft,  0.07f);
                Gizmos.DrawWireSphere(wRight, 0.07f);

                // Подсвечиваем точку на диагонали для текущей позиции игрока
                if (playerTransform != null)
                {
                    float t = Mathf.InverseLerp(wLeft.x, wRight.x, playerTransform.position.x);
                    t = Mathf.Clamp01(t);
                    Vector3 mid = Vector3.Lerp(wLeft, wRight, t);
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(mid, 0.1f);
                    Gizmos.DrawLine(mid, playerTransform.position);
                }
                break;
            }

            case DepthMode.Horizontal:
            {
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float h      = sr.bounds.size.y;
                    float offset = offsetIsRelative ? depthYOffset * h : depthYOffset;
                    float y      = sr.bounds.min.y + offset;
                    float xMin   = sr.bounds.min.x;
                    float xMax   = sr.bounds.max.x;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(new Vector3(xMin, y), new Vector3(xMax, y));
                    Gizmos.DrawWireSphere(new Vector3((xMin + xMax) * 0.5f, y), 0.07f);
                    Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
                    Gizmos.DrawLine(new Vector3(xMin, sr.bounds.min.y), new Vector3(xMax, sr.bounds.min.y));
                }
                break;
            }

            default:
            {
                Gizmos.color = Color.yellow;
                Vector3 p = transform.position + (Vector3)(Vector2)objectPivotOffset;
                Gizmos.DrawWireSphere(p, 0.07f);
                break;
            }
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