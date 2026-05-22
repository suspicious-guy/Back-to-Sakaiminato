using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRegistrar : MonoBehaviour
{
    [Tooltip("Если true и на объекте нет Collider2D, компонент добавит BoxCollider2D автоматически.")]
    [SerializeField] private bool autoAddCollider = true;

    [Tooltip("Размер автоматически добавленного BoxCollider2D.\n" +
             "Подбери под размер своего спрайта в мировых единицах.")]
    [SerializeField] private Vector2 autoColliderSize = new Vector2(0.9f, 0.5f);

    [Tooltip("Смещение центра автоколлайдера относительно пивота спрайта.\n" +
             "В изометрии часто нужно сдвинуть вверх на 0.1–0.2.")]
    [SerializeField] private Vector2 autoColliderOffset = new Vector2(0f, 0.1f);

    [Tooltip("Если true — перестраивать PathfindingGrid при уничтожении этого объекта.\n" +
             "Нужно, если объекты могут исчезать во время игры.")]
    [SerializeField] private bool rebuildOnDestroy = true;


    private void Awake()
    {
        ValidateLayer();
        EnsureCollider();
    }

    private void OnDestroy()
    {
        if (rebuildOnDestroy && PathfindingGrid.Instance != null)
            PathfindingGrid.Instance.BuildGrid();
    }

    private void ValidateLayer()
    {
        int obstacleLayerIndex = LayerMask.NameToLayer("Obstacle");

        if (obstacleLayerIndex == -1)
        {
            Debug.LogWarning($"[ObstacleRegistrar] Слой 'Obstacle' не найден в проекте! " +
                             $"Создай его в Project Settings → Tags and Layers. " +
                             $"Объект: {gameObject.name}");
            return;
        }

        if (gameObject.layer != obstacleLayerIndex)
        {
            Debug.LogWarning($"[ObstacleRegistrar] Объект '{gameObject.name}' не на слое 'Obstacle' " +
                             $"(текущий слой: '{LayerMask.LayerToName(gameObject.layer)}'). " +
                             $"PathfindingGrid его не увидит.");
        }
    }

    private void EnsureCollider()
    {
        if (!autoAddCollider) return;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) return;

        var box = gameObject.AddComponent<BoxCollider2D>();
        box.size = autoColliderSize;
        box.offset = autoColliderOffset;
        box.isTrigger = false;

        Debug.Log($"[ObstacleRegistrar] Добавлен BoxCollider2D на '{gameObject.name}' " +
                  $"(size={autoColliderSize}, offset={autoColliderOffset}).");
    }
}