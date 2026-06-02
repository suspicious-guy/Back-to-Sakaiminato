using UnityEngine;
using UnityEngine.SceneManagement;

public class TrialAttack : MonoBehaviour
{
    [Header("Движение")]
    public float speed = 200000f;

    [Header("Урон")]
    public float damage = 10f;

    private RectTransform rectTransform;
    private float bottomBoundary;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        bottomBoundary = 90;
    }

    void Update()
    {
        Vector3 newPosition = rectTransform.position;
        newPosition.y -= speed * Time.deltaTime;
        rectTransform.position = newPosition;

        if (rectTransform.position.y < bottomBoundary)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
