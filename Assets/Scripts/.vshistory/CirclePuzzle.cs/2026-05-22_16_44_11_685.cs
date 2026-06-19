using UnityEngine;

public class CirclePuzzle : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D ball;
    public Transform centerTarget;  // пустой объект в центре
    public CircleCollider2D ringCollider;

    [Header("Settings")]
    public float escapeForce = 8f;      // сила убегания от центра
    public float noiseForce = 4f;       // случайный шум (Perlin)
    public float mouseAttractForce = 15f;
    public float mouseRadius = 1.5f;    // радиус притяжения мышки

    private Vector2 noiseOffset;

    void Start()
    {
        noiseOffset = Random.insideUnitCircle * 100f;
    }

    void FixedUpdate()
    {
        ApplyEscapeForce();
        ApplyNoiseForce();
        ApplyMouseForce();
    }

    void ApplyEscapeForce()
    {
        // Шарик постоянно убегает от центра
        Vector2 dir = (ball.position - (Vector2)centerTarget.position).normalized;
        ball.AddForce(dir * escapeForce);
    }

    void ApplyNoiseForce()
    {
        // Случайное блуждание через Perlin noise
        float t = Time.time;
        float nx = Mathf.PerlinNoise(t * 0.7f + noiseOffset.x, 0f) * 2f - 1f;
        float ny = Mathf.PerlinNoise(0f, t * 0.7f + noiseOffset.y) * 2f - 1f;
        ball.AddForce(new Vector2(nx, ny) * noiseForce);
    }

    void ApplyMouseForce()
    {
        // Мышка притягивает шарик
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toMouse = mouseWorld - ball.position;
        float dist = toMouse.magnitude;

        if (dist < mouseRadius)
        {
            float strength = (1f - dist / mouseRadius) * mouseAttractForce;
            ball.AddForce(toMouse.normalized * strength);
        }
    }
}