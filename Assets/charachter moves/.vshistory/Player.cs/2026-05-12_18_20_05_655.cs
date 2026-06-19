using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Collections.Unicode;

public class Player : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 8f;

    private float currentSpeed;
    private bool isRunning = false;
    private Vector2 targetPosition;
    private bool hasTarget;
    private Rigidbody2D rb;

    private List<Vector2> currentPath;
    private int currentPathIndex;
    private Vector2 lastMouseTarget;

    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            isRunning = !isRunning;
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            Debug.Log($"Текущая скорость: {currentSpeed}, бег: {isRunning}");
        }

        if (GameInput.Instance.TryGetClickPosition(out Vector2 clickPos))
        {
            lastMouseTarget = clickPos;
            StartMouseMovement(clickPos);
        }
    }
    private void FixedUpdate()
    {
        float speedToUse = currentSpeed;
        if (hasTarget)
        {
            if (currentPath != null)
            {
                currentPath = null;
                Debug.Log("����� ������� �� ������ ����������");
            }

            rb.MovePosition(rb.position + direction * (speedToUse * Time.fixedDeltaTime));
        }
        else
        {
            Vector2 inputVector = GameInput.Instance.GetMovementVector();
            inputVector = inputVector.normalized;

            if (inputVector != Vector2.zero)
            {
                Debug.Log($"Движение WASD, скорость: {speedToUse}");
            }

            rb.MovePosition(rb.position + inputVector * (speedToUse * Time.fixedDeltaTime));
        }
    }

    private void FixedUpdate()
    {
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            MoveAlongPath();
        }
        else if (moveDirection != Vector2.zero)
        {
            TryMoveWithCollision(moveDirection);
        }
    }


    private void StartMouseMovement(Vector2 target)
    {
        if (grid == null)
        {
            Debug.LogError("IsometricGrid �� �������� � ����������!");
            return;
        }

        Vector2Int targetCell = grid.WorldToCell(target);

        if (grid.IsWalkable(targetCell))
        {
            List<Vector2> path = grid.FindPath(transform.position, target);
            if (path != null && path.Count > 0)
            {
                currentPath = path;
                currentPathIndex = 0;
                Debug.Log($"���� ������. �����: {path.Count}");
                return;
            }
        }
        MoveAsCloseAsPossible(target);
    }

    private void MoveAsCloseAsPossible(Vector2 target)
    {
        Vector2Int targetCell = grid.WorldToCell(target);
        Vector2Int nearest = grid.FindNearestWalkable(targetCell);

        if (nearest.x != -1)
        {
            Vector2 reachableTarget = grid.CellToWorld(nearest);
            List<Vector2> path = grid.FindPath(transform.position, reachableTarget);

            if (path != null && path.Count > 0)
            {
                currentPath = path;
                currentPathIndex = 0;
                Debug.Log($"��� �� ��������� ��������� �����. �����: {path.Count}");
            }
        }
    }

    private void MoveAlongPath()
    {
        Vector2 targetPos = currentPath[currentPathIndex];
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, movingSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, targetPos) < 0.05f)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                currentPath = null;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void TryMoveWithCollision(Vector2 direction)
    {
        Vector2 newPos = rb.position + direction * movingSpeed * Time.fixedDeltaTime;

        if (IsPositionWalkable(newPos))
        {
            rb.MovePosition(newPos);
        }
        else
        {
            Vector2 newPosX = new Vector2(newPos.x, rb.position.y);
            if (IsPositionWalkable(newPosX))
                rb.MovePosition(newPosX);

            Vector2 newPosY = new Vector2(rb.position.x, newPos.y);
            if (IsPositionWalkable(newPosY))
                rb.MovePosition(newPosY);
        }
    }

    private bool IsPositionWalkable(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, 0.2f, blockingLayers);
        return hit == null;
    }

}