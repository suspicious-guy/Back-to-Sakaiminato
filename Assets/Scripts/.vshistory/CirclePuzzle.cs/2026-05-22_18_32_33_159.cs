using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CirclePuzzle : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D item;
    public Transform centerTarget;
    public Image progressBar;        // UI Image, Image Type = Filled, Fill Method = Vertical
    public float successRadius = 0.5f;
    public float totalHoldTime = 10f;

    [Header("Physics")]
    public float escapeForce = 8f;
    public float noiseForce = 4f;
    public float mouseAttractForce = 15f;
    public float mouseRadius = 1.5f;

    [Header("Colors")]
    public Color colorFull = new Color(0.2f, 0.8f, 0.3f);   // зелёный
    public Color colorEmpty = new Color(0.85f, 0.2f, 0.2f);  // красный

    private Vector2 noiseOffset;
    private float holdProgress = 0f;   // 0..1
    private bool puzzleSolved = false;

    void Start()
    {
        noiseOffset = Random.insideUnitCircle * 100f;
    }

    void FixedUpdate()
    {
        if (puzzleSolved) return;
        ApplyEscapeForce();
        ApplyNoiseForce();
        ApplyMouseForce();

        float maxSpeed = 4f;
        item.linearVelocity = Vector2.ClampMagnitude(item.linearVelocity, maxSpeed);
    }

    void Update()
    {
        if (puzzleSolved) return;
        UpdateProgress();
        UpdateBar();
    }

    void ApplyEscapeForce()
    {
        Vector2 dir = (item.position - (Vector2)centerTarget.position).normalized;
        item.AddForce(dir * escapeForce);
    }

    void ApplyNoiseForce()
    {
        float t = Time.time;
        float nx = Mathf.PerlinNoise(t * 0.7f + noiseOffset.x, 0f) * 2f - 1f;
        float ny = Mathf.PerlinNoise(0f, t * 0.7f + noiseOffset.y) * 2f - 1f;
        item.AddForce(new Vector2(nx, ny) * noiseForce);
    }

    void ApplyMouseForce()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        Vector2 toMouse = mouseWorld - item.position;
        float dist = toMouse.magnitude;

        if (dist < mouseRadius)
        {
            float strength = (1f - dist / mouseRadius) * mouseAttractForce;
            item.AddForce(toMouse.normalized * strength);
        }
    }

    void UpdateProgress()
    {
        float dist = Vector2.Distance(item.position, centerTarget.position);
        bool inCenter = dist < successRadius;

        if (inCenter)
        {
            // Нарастает за 10 секунд
            holdProgress += Time.deltaTime / totalHoldTime;
        }
        else
        {
            // Убывает вдвое быстрее
            holdProgress -= (Time.deltaTime / totalHoldTime) * 2f;
        }

        holdProgress = Mathf.Clamp01(holdProgress);

        if (holdProgress >= 1f)
        {
            puzzleSolved = true;
            OnPuzzleSolved();
        }
    }

    void UpdateBar()
    {
        progressBar.fillAmount = holdProgress;
        // Плавный переход цвета: красный → зелёный
        progressBar.color = Color.Lerp(colorEmpty, colorFull, holdProgress);
    }

    void OnPuzzleSolved()
    {
        Debug.Log("Головоломка решена!");
        // Здесь вызывай свою логику завершения
    }
}