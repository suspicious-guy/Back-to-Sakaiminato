using UnityEngine;

/// <summary>
/// Динамическая сортировка по глубине для изометрической 2D-сцены.
///
/// Повесь этот компонент на объект (ящик, дерево, NPC и т.д.).
/// Скрипт каждый кадр пересчитывает Order in Layer у себя и у игрока
/// так, чтобы ближайший к камере (с бо́льшим Y) рисовался поверх.
///
/// Требования:
///   - На объекте должен быть SpriteRenderer.
///   - На игроке должен быть SpriteRenderer.
///   - Оба SpriteRenderer должны быть на одном Sorting Layer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class IsometricDepthSorter : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Transform игрока. Если не заполнено — ищется по тегу 'Player' автоматически.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("SpriteRenderer игрока. Заполняется автоматически, если не задан.")]
    [SerializeField] private SpriteRenderer playerRenderer;

    [Header("Точка сравнения глубины")]
    [Tooltip("Смещение от пивота объекта до его 'основания' на земле.\n" +
             "В изометрии пивот спрайта часто стоит в центре, а нужно сравнивать\n" +
             "по нижней точке (основанию объекта).\n" +
             "Например, если спрайт высотой 1 unit и пивот в центре — ставь Y = -0.5.")]
    [SerializeField] private Vector2 objectPivotOffset = new Vector2(0f, -0.5f);

    [Tooltip("Смещение точки сравнения для игрока (обычно = его ноги).")]
    [SerializeField] private Vector2 playerPivotOffset = new Vector2(0f, -1f);

    [Header("Базовые значения Order in Layer")]
    [Tooltip("Order объекта когда игрок за ним (объект рисуется ПОВЕРХ игрока).")]
    [SerializeField] private int orderWhenInFront = 2;

    [Tooltip("Order объекта когда игрок перед ним (игрок рисуется ПОВЕРХ объекта).")]
    [SerializeField] private int orderWhenBehind = 0;

    [Tooltip("Order игрока когда он за объектом.")]
    [SerializeField] private int playerOrderBehind = 1;

    [Tooltip("Order игрока когда он перед объектом.")]
    [SerializeField] private int playerOrderInFront = 3;

    [Header("Альтернативный режим: непрерывная сортировка")]
    [Tooltip("Если true — Order рассчитывается как -Y * multiplier вместо двух фиксированных значений.\n" +
             "Лучше работает когда в сцене много объектов, которые должны перекрывать друг друга правильно.")]
    [SerializeField] private bool useContinuousSorting = false;

    [Tooltip("Множитель для непрерывной сортировки. Обычно 10–100.")]
    [SerializeField] private float sortingMultiplier = 10f;

    // ─── Компоненты ───────────────────────────────────────────────────────────

    private SpriteRenderer spriteRenderer;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Автопоиск игрока по тегу, если не задан вручную
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
        // LateUpdate гарантирует, что игрок уже переместился в этом кадре
        if (playerTransform == null) return;

        if (useContinuousSorting)
            UpdateContinuous();
        else
            UpdateBinary();
    }

    // ─── Бинарный режим ───────────────────────────────────────────────────────

    /// <summary>
    /// Сравнивает Y-координаты основания объекта и ног игрока.
    /// Кто ниже по Y (дальше от камеры в изометрии) — тот "дальше".
    /// </summary>
    private void UpdateBinary()
    {
        // Точка сравнения: основание объекта
        float objectBaseY = transform.position.y + objectPivotOffset.y;

        // Точка сравнения: ноги игрока
        float playerBaseY = playerTransform.position.y + playerPivotOffset.y;

        if (playerBaseY < objectBaseY)
        {
            // Игрок дальше от камеры (ниже по Y) → он за объектом
            // Объект рисуется поверх игрока
            spriteRenderer.sortingOrder = orderWhenInFront;
            if (playerRenderer != null)
                playerRenderer.sortingOrder = playerOrderBehind;
        }
        else
        {
            // Игрок ближе к камере (выше по Y) → он перед объектом
            // Игрок рисуется поверх объекта
            spriteRenderer.sortingOrder = orderWhenBehind;
            if (playerRenderer != null)
                playerRenderer.sortingOrder = playerOrderInFront;
        }
    }

    // ─── Непрерывный режим ────────────────────────────────────────────────────

    /// <summary>
    /// Устанавливает Order пропорционально -Y.
    /// Работает корректно при большом числе объектов в сцене.
    ///
    /// Принцип: в изометрии объекты с меньшим Y находятся дальше от камеры.
    /// Чтобы дальние рисовались под ближними — нужен больший Order у дальних.
    /// Поэтому: Order = RoundToInt(-Y * multiplier).
    /// </summary>
    private void UpdateContinuous()
    {
        float objectBaseY = transform.position.y + objectPivotOffset.y;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-objectBaseY * sortingMultiplier);

        if (playerRenderer != null)
        {
            float playerBaseY = playerTransform.position.y + playerPivotOffset.y;
            playerRenderer.sortingOrder = Mathf.RoundToInt(-playerBaseY * sortingMultiplier);
        }
    }

    // ─── Гизмо ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Показывает точку сравнения глубины в Scene-окне
        Gizmos.color = Color.yellow;
        Vector3 basePoint = transform.position + (Vector3)objectPivotOffset;
        Gizmos.DrawWireSphere(basePoint, 0.08f);

        // Горизонтальная линия — "линия глубины" объекта
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawLine(basePoint + Vector3.left * 0.5f, basePoint + Vector3.right * 0.5f);
    }
#endif
}