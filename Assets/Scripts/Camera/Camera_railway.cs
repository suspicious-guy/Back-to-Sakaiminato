using UnityEngine;

public class TopDownCamera3_4 : MonoBehaviour
{
    [Header("Цель (игрок)")]
    public Transform player;              

    [Header("Настройки следования")]
    public float smoothSpeed = 5f;       
    public Vector3 offset = new Vector3(0, 5, -8);  

    [Header("Ограничения (опционально)")]
    public bool limitBounds = false;
    public Vector2 minBounds;              
    public Vector2 maxBounds;              

    void LateUpdate()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
            else
                return;
        }

        Vector3 targetPosition = player.position + offset;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        if (limitBounds)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
                transform.position.z
            );
        }
    }
}