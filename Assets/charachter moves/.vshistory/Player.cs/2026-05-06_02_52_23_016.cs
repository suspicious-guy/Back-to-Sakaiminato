using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField] private float movingSpeed = 4f;
    [SerializeField] private LayerMask blockingLayers;
    [SerializeField] private IsometricGrid grid;

    private Rigidbody2D rb;

    private List<Vector2> currentPath;
    private int currentPathIndex;
    private Vector2 lastMouseTarget;

    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (GameInput.Instance.TryGetClickPosition(out Vector2 clickPos))
        {
            lastMouseTarget = clickPos;
            StartMouseMovement(clickPos);
        }

        Vector2 input = GameInput.Instance.GetMovementVector();
        if (input != Vector2.zero)
        {
            if (currentPath != null)
            {
                currentPath = null;
                Debug.Log("Режим изменён на ручное управление");
            }
            moveDirection = input.normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
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
            Debug.LogError("IsometricGrid не назначен в инспекторе!");
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
                Debug.Log($"Путь найден. Шагов: {path.Count}");
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
                Debug.Log($"Иду до ближайшей доступной точки. Шагов: {path.Count}");
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