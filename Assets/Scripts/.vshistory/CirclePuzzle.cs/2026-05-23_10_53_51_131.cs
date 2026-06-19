using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CirclePuzzle : MonoBehaviour
{
    [Header("References")]
    public Camera puzzleCamera;
    public Rigidbody2D item;
    public Transform centerTarget;
    public Image progressBar;
    public float successRadius = 0.5f;
    public float totalHoldTime = 10f;

    [Header("Physics")]
    public float escapeForce = 8f;
    public float noiseForce = 4f;
    public float mouseAttractForce = 15f;
    public float mouseRadius = 1.5f;
    public float maxSpeed = 4f;
    public float directionChangeInterval = 1.2f;

    [Header("Boundaries")]
    public float failRadius = 2.3f;

    [Header("Colors")]
    public Color colorFull = new Color(0.2f, 0.8f, 0.3f);
    public Color colorEmpty = new Color(0.85f, 0.2f, 0.2f);

    [Header("Events")]
    public UnityEvent onFail;
    public UnityEvent onSolved;

    private Vector2 noiseOffset;
    private float holdProgress = 0f;
    private bool puzzleSolved = false;
    private bool isFailed = false;

    private Vector2 currentEscapeDir;
    private float dirChangeTimer;

    void Start()
    {
        noiseOffset = Random.insideUnitCircle * 80f;
        PickNewEscapeDirection();
    }

    void FixedUpdate()
    {
        if (puzzleSolved || isFailed) return;

        ApplyEscapeForce();
        ApplyNoiseForce();
        ApplyMouseForce();

        item.linearVelocity = Vector2.ClampMagnitude(item.linearVelocity, maxSpeed);
    }

    void Update()
    {
        if (puzzleSolved || isFailed) return;

        CheckBoundary();
        UpdateProgress();
        UpdateBar();
    }

    void PickNewEscapeDirection()
    {
        Vector2 fromCenter = item.position - (Vector2)centerTarget.position;
        float baseAngle = fromCenter.magnitude > 0.01f
            ? Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg
            : Random.Range(0f, 360f);

        float randomAngle = baseAngle + Random.Range(-90f, 90f);
        float rad = randomAngle * Mathf.Deg2Rad;
        currentEscapeDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        dirChangeTimer = directionChangeInterval + Random.Range(-0.3f, 0.3f);
    }


    void ApplyEscapeForce()
    {
        dirChangeTimer -= Time.fixedDeltaTime;
        if (dirChangeTimer <= 0f)
            PickNewEscapeDirection();

        item.AddForce(currentEscapeDir * escapeForce);
    }

    void ApplyNoiseForce()
    {
        float t = Time.time;
        float nx = Mathf.PerlinNoise(t * 0.7f + noiseOffset.x, 0f) * 2f - 1f;
        float ny = Mathf.PerlinNoise(0f, t * 0.7f + noiseOffset.y) * 2f - 1f;

        Vector2 noiseDir = new Vector2(nx, ny);
        if (noiseDir.magnitude > 0.01f)
            noiseDir = noiseDir.normalized;

        item.AddForce(noiseDir * noiseForce);
    }

    void ApplyMouseForce()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = puzzleCamera.ScreenToWorldPoint(mouseScreen);
        Vector2 toMouse = mouseWorld - item.position;
        float dist = toMouse.magnitude;

        if (dist < mouseRadius)
        {
            float strength = (1f - dist / mouseRadius) * mouseAttractForce;
            item.AddForce(toMouse.normalized * strength);
        }
    }


    void CheckBoundary()
    {
        float dist = Vector2.Distance(item.position, centerTarget.position);
        if (dist > failRadius)
            TriggerFail();
    }

    void TriggerFail()
    {
        isFailed = true;

        item.linearVelocity = Vector2.zero;
        item.angularVelocity = 0f;
        item.bodyType = RigidbodyType2D.Kinematic;

        SceneManager.UnloadSceneAsync(gameObject.scene.name);

        onFail.Invoke();
    }


    void UpdateProgress()
    {
        float dist = Vector2.Distance(item.position, centerTarget.position);
        bool inCenter = dist < successRadius;

        if (inCenter)
            holdProgress += Time.deltaTime / totalHoldTime;
        else
            holdProgress -= (Time.deltaTime / totalHoldTime) * 2f;

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
        progressBar.color = Color.Lerp(colorEmpty, colorFull, holdProgress);
    }

    void OnPuzzleSolved()
    {
        SceneManager.UnloadSceneAsync(gameObject.scene.name);
        onSolved.Invoke();
        Debug.Log("Головоломка решена!");
    }

    public void ResetPuzzle()
    {
        isFailed = false;
        puzzleSolved = false;
        holdProgress = 0f;

        item.bodyType = RigidbodyType2D.Dynamic;
        item.position = centerTarget.position;
        item.linearVelocity = Vector2.zero;
        item.angularVelocity = 0f;

        PickNewEscapeDirection();
    }
}