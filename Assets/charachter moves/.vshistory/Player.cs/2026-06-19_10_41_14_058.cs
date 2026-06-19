using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Скорость")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Навигация")]
    [SerializeField] private float waypointReachDistance = 0.08f;
    [SerializeField] private float smoothMovement = 15f;  // для WASD

    private Rigidbody2D rb;

    private float currentSpeed;
    private bool isRunning;
    private bool movementEnabled = true;

    private List<Vector2> path;
    private int pathIndex;
    private bool hasPath;

    private Vector2 wasdInput;

    void Start()
    {
        transform.position = new Vector3(1.16, -0.04, 0);
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        HandleRunToggle();
        HandleWASD();
        HandleClick();
    }

    private void FixedUpdate()
    {
        if (hasPath)
            MoveAlongPath();      // обычное движение для кликов
        else
            MoveByWASDLerp();     // Lerp только для WASD
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;

        if (!movementEnabled)
        {
            CancelPath();
            wasdInput = Vector2.zero;
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleRunToggle()
    {
        bool shiftPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        if (shiftPressed != isRunning)
        {
            isRunning = shiftPressed;
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }
    }

    private void HandleWASD()
    {
        if (!movementEnabled)
        {
            wasdInput = Vector2.zero;
            return;
        }

        wasdInput = GameInput.Instance.GetMovementVector();

        if (wasdInput != Vector2.zero)
            CancelPath();
    }

    private void HandleClick()
    {
        if (!movementEnabled) return;

        if (!GameInput.Instance.TryGetClickPosition(out Vector2 clickPos)) return;

        List<Vector2> newPath = PathfindingGrid.Instance.FindPath(rb.position, clickPos);

        if (newPath != null && newPath.Count > 0)
        {
            path = newPath;
            pathIndex = 0;
            hasPath = true;
        }
        else
        {
            CancelPath();
        }
    }

    // Обычное движение по пути (без Lerp)
    private void MoveAlongPath()
    {
        if (pathIndex >= path.Count)
        {
            CancelPath();
            return;
        }

        Vector2 target = path[pathIndex];
        Vector2 direction = (target - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, target);

        if (distance < waypointReachDistance)
        {
            rb.MovePosition(target);
            pathIndex++;

            if (pathIndex >= path.Count)
            {
                CancelPath();
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        rb.MovePosition(rb.position + direction * (currentSpeed * Time.fixedDeltaTime));
    }

    private void CancelPath()
    {
        hasPath = false;
        path = null;
    }

    // Lerp только для WASD
    private void MoveByWASDLerp()
    {
        if (!movementEnabled) return;

        Vector2 dir = wasdInput.normalized;
        if (dir == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 targetPos = rb.position + dir * (currentSpeed * Time.fixedDeltaTime);

        if (PathfindingGrid.Instance != null && !PathfindingGrid.Instance.IsWalkable(targetPos))
        {
            Vector2 posX = rb.position + new Vector2(dir.x, 0f) * (currentSpeed * Time.fixedDeltaTime);
            if (dir.x != 0f && PathfindingGrid.Instance.IsWalkable(posX))
            {
                rb.MovePosition(posX);
                return;
            }

            Vector2 posY = rb.position + new Vector2(0f, dir.y) * (currentSpeed * Time.fixedDeltaTime);
            if (dir.y != 0f && PathfindingGrid.Instance.IsWalkable(posY))
            {
                rb.MovePosition(posY);
                return;
            }

            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Lerp движение для плавности WASD
        float step = smoothMovement * Time.fixedDeltaTime;
        Vector2 newPosition = Vector2.Lerp(rb.position, targetPos, Mathf.Min(step, 1f));
        rb.MovePosition(newPosition);
    }
}