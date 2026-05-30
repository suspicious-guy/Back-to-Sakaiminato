using UnityEngine;

/// <summary>
/// Динамическая сортировка по глубине для изометрической 2D-сцены.
///
/// Повесь этот компонент на объект (ящик, здание, NPC и т.д.).
/// Скрипт каждый кадр пересчитывает Order in Layer так,
/// чтобы ближайший к камере объект рисовался поверх.
///
/// Для больших объектов (здания, навесы) используй режим
/// useSpriteBottomEdge = true — тогда сравнение идёт по нижнему
/// краю спрайта, а не по пивоту. Это решает проблему когда объект
/// широкий и занимает много клеток.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class IsometricDepthSorter : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Transform игрока. Если не заполнено — ищется по тегу 'Player' автоматически.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("SpriteRenderer игрока. Заполняется автоматически, если не задан.")]
    [SerializeField] private SpriteRenderer playerRenderer;

    [Header("Точка сравнения глубины объекта")]
    [Tooltip("true  — берёт нижний край bounds спрайта как базу, затем добавляет depthYOffset.\n" +
             "false — берёт пивот + ручное смещение.")]
    [SerializeField] private bool useSpriteBottomEdge = true;

    [Tooltip("Смещение по Y от нижнего края спрайта вверх к передней стене.\n" +
             "Значение 0 = самый нижний пиксель спрайта.\n" +
             "Увеличивай (0.1, 0.2 ...) пока жёлтая линия не встанет на переднюю стену объекта.\n" +
             "Используй 0..1 как долю высоты спрайта (удобно) или абсолютные мировые единицы.")]
    [SerializeField] private float depthYOffset = 0f;

    [Tooltip("Если true — depthYOffset интерпретируется как доля высоты спрайта (0 = низ, 1 = верх).\n" +
             "Если false — абсолютные мировые единицы.")]
    [SerializeField] private bool offsetIsRelative = true;

    [Tooltip("Ручное смещение от пивота (используется только если useSpriteBottomEdge = false).\n" +
             "При пивоте Bottom Center оставь (0, 0).")]
    [SerializeField] private Vector2 objectPivotOffset = new Vector2(0f, 0f);

    [Tooltip("Ручное смещение для игрока. При пивоте Bottom Center оставь (0, 0).")]
    [SerializeField] private Vector2 playerPivotOffset = new Vector2(0f, 0f);

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
    [Tooltip("Если true — Order = -Y * multiplier.\n" +
             "Лучше когда в сцене много объектов разного размера.")]
    [SerializeField] private bool useContinuousSorting = false;

    [SerializeField] private float sortingMultiplier = 10f;

    // ─────────────────────────────────────────────────────────────────────────

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

        if (useContinuousSorting)
            UpdateContinuous();
        else
            UpdateBinary();
    }

    // ─── Получение Y-точки сравнения ─────────────────────────────────────────

    /// <summary>
    /// Возвращает Y-координату "передней грани" объекта для сравнения глубины.
    /// depthYOffset позволяет сдвинуть линию сравнения вверх от нижнего края
    /// до передней стены — именно там объект визуально "начинается" для игрока.
    /// </summary>
    private float GetObjectDepthY()
    {
        if (useSpriteBottomEdge && spriteRenderer != null)
        {
            float minY = spriteRenderer.bounds.min.y;
            float height = spriteRenderer.bounds.size.y;
            float offset = offsetIsRelative ? depthYOffset * height : depthYOffset;
            return minY + offset;
        }
        return transform.position.y + objectPivotOffset.y;
    }

    private float GetPlayerDepthY()
    {
        if (playerRenderer != null)
            return playerRenderer.bounds.min.y;
        return playerTransform.position.y + playerPivotOffset.y;
    }

    // ─── Бинарный режим ───────────────────────────────────────────────────────

    private void UpdateBinary()
    {
        float objectY = GetObjectDepthY();
        float playerY = GetPlayerDepthY();

        if (playerY < objectY)
        {
            // Игрок дальше от камеры → объект перекрывает игрока
            spriteRenderer.sortingOrder = orderWhenInFront;
            if (playerRenderer != null)
                playerRenderer.sortingOrder = playerOrderBehind;
        }
        else
        {
            // Игрок ближе к камере → игрок перекрывает объект
            spriteRenderer.sortingOrder = orderWhenBehind;
            if (playerRenderer != null)
                playerRenderer.sortingOrder = playerOrderInFront;
        }
    }

    // ─── Непрерывный режим ────────────────────────────────────────────────────

    private void UpdateContinuous()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-GetObjectDepthY() * sortingMultiplier);

        if (playerRenderer != null)
            playerRenderer.sortingOrder = Mathf.RoundToInt(-GetPlayerDepthY() * sortingMultiplier);
    }

    // ─── Гизмо ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (useSpriteBottomEdge && sr != null)
        {
            float height  = sr.bounds.size.y;
            float offset  = offsetIsRelative ? depthYOffset * height : depthYOffset;
            float y       = sr.bounds.min.y + offset;
            float xMin    = sr.bounds.min.x;
            float xMax    = sr.bounds.max.x;

            // Жёлтая линия = реальная граница сравнения глубины
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(xMin, y, 0f), new Vector3(xMax, y, 0f));
            Gizmos.DrawWireSphere(new Vector3((xMin + xMax) * 0.5f, y, 0f), 0.08f);

            // Серая линия = нижний край спрайта (offset = 0)
            Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
            Gizmos.DrawLine(new Vector3(xMin, sr.bounds.min.y, 0f),
                            new Vector3(xMax, sr.bounds.min.y, 0f));
        }
        else
        {
            // Ручной режим — показываем точку пивота со смещением
            Gizmos.color = Color.yellow;
            Vector3 basePoint = transform.position + (Vector3)objectPivotOffset;
            Gizmos.DrawWireSphere(basePoint, 0.08f);
            Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
            Gizmos.DrawLine(basePoint + Vector3.left * 0.5f, basePoint + Vector3.right * 0.5f);
        }
    }
#endif

    public void SetPlayer(GameObject player)
    {
        if (player != null)
        {
            playerTransform = player.transform;
            playerRenderer = player.GetComponent<SpriteRenderer>();
        }
    }
}