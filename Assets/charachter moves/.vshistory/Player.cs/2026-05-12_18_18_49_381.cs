using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Скорость")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 8f;

    [Header("Навигация")]
    [Tooltip("Расстояние до путевой точки, при котором считаем её достигнутой")]
    [SerializeField] private float waypointReachDistance = 0.08f;

    private float currentSpeed;
    private bool isRunning;


    private List<Vector2> path;
    private int pathIndex;
    private bool hasPath;


    private Vector2 wasdInput;


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
            MoveAlongPath();
        else
            MoveByWASD();
    }


    private void HandleRunToggle()
    {
        bool shiftHeld = Keyboard.current.leftShiftKey.isPressed;
        if (shiftHeld != isRunning)
        {
            isRunning = shiftHeld;
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }
    }

    private void HandleWASD()
    {
        wasdInput = GameInput.Instance.GetMovementVector();

        if (wasdInput != Vector2.zero)
            CancelPath();
    }

    private void HandleClick()
    {
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


    private void MoveAlongPath()
    {
        if (pathIndex >= path.Count) { CancelPath(); return; }

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


    private void MoveByWASD()
    {
        Vector2 dir = wasdInput.normalized;
        if (dir == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 newPos = rb.position + dir * (currentSpeed * Time.fixedDeltaTime);

        if (PathfindingGrid.Instance != null && !PathfindingGrid.Instance.IsWalkable(newPos))
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

        rb.MovePosition(newPos);
    }
}