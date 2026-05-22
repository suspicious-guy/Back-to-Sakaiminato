using UnityEngine;

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
    [SerializeField] private Vector2 objectPivotOffset = new Vector2(0f, 0f);

    [Tooltip("Смещение точки сравнения для игрока (обычно = его ноги).")]
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
    [Tooltip("Если true — Order рассчитывается как -Y * multiplier вместо двух фиксированных значений.\n" +
             "Лучше работает когда в сцене много объектов, которые должны перекрывать друг друга правильно.")]
    [SerializeField] private bool useContinuousSorting = false;

    [Tooltip("Множитель для непрерывной сортировки. Обычно 10–100.")]
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

    private void UpdateBinary()
    {
        float objectBaseY = transform.position.y + objectPivotOffset.y;

        float playerBaseY = playerTransform.position.y + playerPivotOffset.y;

        if (playerBaseY < objectBaseY)
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
        float objectBaseY = transform.position.y + objectPivotOffset.y;
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-objectBaseY * sortingMultiplier);

        if (playerRenderer != null)
        {
            float playerBaseY = playerTransform.position.y + playerPivotOffset.y;
            playerRenderer.sortingOrder = Mathf.RoundToInt(-playerBaseY * sortingMultiplier);
        }
    }


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

    public void SetPlayer(GameObject player)
    {
        if (player != null)
        {
            playerTransform = player.transform;
            playerRenderer = player.GetComponent<SpriteRenderer>();
            Debug.Log($"[IsometricDepthSorter] '{gameObject.name}': Игрок установлен");
        }
    }
}