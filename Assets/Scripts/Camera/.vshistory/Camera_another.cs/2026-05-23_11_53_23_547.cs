using UnityEngine;

public class TopDownCamera3_4_ : MonoBehaviour
{
    [Header("Цель (игрок)")]
    public Transform player;

    [Header("Настройки следования")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 5, -8);

    [Header("Границы игрового поля (мировые координаты)")]
    [Tooltip("Включить ограничение камеры по полю")]
    public bool limitBounds = true;

    [Tooltip("Минимальная X-координата левого края поля")]
    public float fieldMinX = -20f;
    [Tooltip("Максимальная X-координата правого края поля")]
    public float fieldMaxX = 20f;
    [Tooltip("Минимальная Y-координата нижнего края поля")]
    public float fieldMinY = -10f;
    [Tooltip("Максимальная Y-координата верхнего края поля")]
    public float fieldMaxY = 10f;

    [Header("Стартовая позиция камеры (до входа игрока в поле)")]
    [Tooltip("Камера будет стоять здесь, пока игрок не войдёт в поле")]
    public Vector3 initialCameraPosition;

    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (initialCameraPosition == Vector3.zero)
            initialCameraPosition = transform.position;
    }

    void LateUpdate()
    {
        // Автоматический поиск игрока, если не назначен
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
            else
                return;
        }

        // Обновляем размеры камеры
        UpdateCameraBounds();

        Vector3 targetPosition = player.position + offset;

        if (limitBounds)
        {
            float clampMinX = fieldMinX + halfWidth;
            float clampMaxX = fieldMaxX - halfWidth;
            float clampMinY = fieldMinY + halfHeight;
            float clampMaxY = fieldMaxY - halfHeight;

            if (clampMinX > clampMaxX) clampMinX = clampMaxX = (fieldMinX + fieldMaxX) * 0.5f;
            if (clampMinY > clampMaxY) clampMinY = clampMaxY = (fieldMinY + fieldMaxY) * 0.5f;

            bool playerInsideFieldX = player.position.x >= fieldMinX && player.position.x <= fieldMaxX;
            bool playerInsideFieldY = player.position.y >= fieldMinY && player.position.y <= fieldMaxY;

            if (playerInsideFieldX)
                targetPosition.x = Mathf.Clamp(targetPosition.x, clampMinX, clampMaxX);
            else
                targetPosition.x = initialCameraPosition.x;

            if (playerInsideFieldY)
                targetPosition.y = Mathf.Clamp(targetPosition.y, clampMinY, clampMaxY);
            else
                targetPosition.y = initialCameraPosition.y;

            targetPosition.z = player.position.z + offset.z;
        }

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;
    }

    private void UpdateCameraBounds()
    {
        if (cam != null && cam.orthographic)
        {
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }
        else if (cam != null)
        {
            halfHeight = Mathf.Abs(offset.y) * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            halfWidth = halfHeight * cam.aspect;
        }
    }

    // Публичный метод для ручной установки игрока (например, из SpawnPoint)
    public void SetPlayer(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            player = playerTransform;
            Debug.Log("✅ Камера: игрок установлен");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (fieldMinX + fieldMaxX) * 0.5f,
            (fieldMinY + fieldMaxY) * 0.5f,


0f
        );
        Vector3 size = new Vector3(
            fieldMaxX - fieldMinX,
            fieldMaxY - fieldMinY,
            0.1f
        );
        Gizmos.DrawWireCube(center, size);
    }
}