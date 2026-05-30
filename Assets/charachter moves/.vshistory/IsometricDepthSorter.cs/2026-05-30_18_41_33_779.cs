using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsometricDepthSorter : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Transform игрока. Если не заполнено — ищется по тегу 'Player' автоматически.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("SpriteRenderer игрока. Заполняется автоматически, если не задан.")]
    [SerializeField] private SpriteRenderer playerRenderer;

    [Header("Точка сравнения глубины объекта")]
    [Tooltip("true  — берёт нижний край bounds спрайта (лучше для больших объектов: зданий, навесов).\n" +
             "false — берёт пивот + ручное смещение (лучше для маленьких объектов: ящиков, деревьев).")]
    [SerializeField] private bool useSpriteBottomEdge = true;

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

    private float GetObjectDepthY()
    {
        if (useSpriteBottomEdge && spriteRenderer != null)
        {
            return spriteRenderer.bounds.min.y;
        }
        return transform.position.y + objectPivotOffset.y;
    }

    private float GetPlayerDepthY()
    {
        if (playerRenderer != null)
            return playerRenderer.bounds.min.y;
        return playerTransform.position.y + playerPivotOffset.y;
    }

    private void UpdateBinary()
    {
        float objectY = GetObjectDepthY();
        float playerY = GetPlayerDepthY();

        if (playerY < objectY)
        {
            spriteRenderer.sortingOrder = orderWhenInFront;
            if (playerRenderer != null)
                playerRenderer.sortingOrder = playerOrderBehind;
        }
        else
        {
            spriteRenderer.sortingOrder = orderWhenBehind;
            if (playerRenderer != null)
                playerRenderer.sortingOrder = playerOrderInFront;
        }
    }

    private void UpdateContinuous()
    {
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-GetObjectDepthY() * sortingMultiplier);

        if (playerRenderer != null)
            playerRenderer.sortingOrder = Mathf.RoundToInt(-GetPlayerDepthY() * sortingMultiplier);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (useSpriteBottomEdge && sr != null)
        {
            // Рисуем линию по нижнему краю спрайта — именно здесь идёт сравнение
            float y    = sr.bounds.min.y;
            float xMin = sr.bounds.min.x;
            float xMax = sr.bounds.max.x;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(xMin, y, 0f), new Vector3(xMax, y, 0f));

            // Маркер в центре нижнего края
            Gizmos.DrawWireSphere(new Vector3((xMin + xMax) * 0.5f, y, 0f), 0.08f);
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