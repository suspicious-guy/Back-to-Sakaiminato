using UnityEngine;
using UnityEngine.InputSystem;

public class CirclePuzzle : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D obj;
    public Transform centerTarget;
    public CircleCollider2D ringCollider;

    [Header("Settings")]
    public float escapeForce = 8f;
    public float noiseForce = 4f;
    public float mouseAttractForce = 15f;
    public float mouseRadius = 1.5f;

    private Vector2 noiseOffset;
    private InputAction mouseAction;

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
        Vector2 dir = (obj.position - (Vector2)centerTarget.position).normalized;
        obj.AddForce(dir * escapeForce);
    }

    void ApplyNoiseForce()
    {
        float t = Time.time;
        float nx = Mathf.PerlinNoise(t * 0.7f + noiseOffset.x, 0f) * 2f - 1f;
        float ny = Mathf.PerlinNoise(0f, t * 0.7f + noiseOffset.y) * 2f - 1f;
        obj.AddForce(new Vector2(nx, ny) * noiseForce);
    }

    void ApplyMouseForce()
    {
        Vector2 mouseScreen = mouseAction.ReadValue<Vector2>();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        float dist = toMouse.magnitude;

        if (dist < mouseRadius)
        {
            float strength = (1f - dist / mouseRadius) * mouseAttractForce;
            obj.AddForce(toMouse.normalized * strength);
        }
    }
}