using UnityEngine;

public class IsometricGrid : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public LayerMask obstacleLayers;
    public bool[,] walkable;


    public bool IsWalkable(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return false;
        return walkable[cell.x, cell.y];
    }
    public void BuildGrid()
    {
        walkable = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = CellToWorld(new Vector2Int(x, y));
                Collider2D hit = Physics2D.OverlapPoint(worldPos, obstacleLayers);
                walkable[x, y] = (hit == null);
            }
        }
    }


    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public float GCost;  // стоимость от старта
        public float HCost;  // эвристика до цели
        public float FCost => GCost + HCost;

        public Node(Vector2Int pos, Node par, float g, float h)
        {
            position = pos;
            parent = par;
            GCost = g;
            HCost = h;
        }
    }

    public List<Vector2> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector2Int startCell = WorldToCell(startWorld);
        Vector2Int targetCell = WorldToCell(targetWorld);

        // Проверка: можно ли вообще стоять на старте/цели
        if (!IsWalkable(startCell) || !IsWalkable(targetCell))
            return null;

        // Открытый и закрытый списки
        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();

        // Стартовый узел
        Node startNode = new Node(startCell, null, 0, Heuristic(startCell, targetCell));
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Берём узел с минимальной F-стоимостью
            Node current = openSet.OrderBy(n => n.FCost).First();

            // Достигли цели?
            if (current.position == targetCell)
                return ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current.position);

            // Проверяем соседей
            foreach (Vector2Int neighborOffset in GetNeighborOffsets())
            {
                Vector2Int neighborPos = current.position + neighborOffset;

                // Пропускаем, если вне сетки или непроходим
                if (!IsWalkable(neighborPos))
                    continue;

                // Пропускаем, если уже в закрытом списке
                if (closedSet.Contains(neighborPos))
                    continue;

                // Стоимость пути до соседа
                float tentativeGCost = current.GCost + Vector2Int.Distance(current.position, neighborPos);

                // Ищем соседа в открытом списке
                Node neighborNode = openSet.FirstOrDefault(n => n.position == neighborPos);

                if (neighborNode == null)
                {
                    // Новый узел
                    float hCost = Heuristic(neighborPos, targetCell);
                    Node newNode = new Node(neighborPos, current, tentativeGCost, hCost);
                    openSet.Add(newNode);
                }
                else if (tentativeGCost < neighborNode.GCost)
                {
                    // Обновляем существующий узел
                    neighborNode.parent = current;
                    neighborNode.GCost = tentativeGCost;
                    neighborNode.HCost = Heuristic(neighborPos, targetCell);
                }
            }
        }

        // Путь не найден
        return null;
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> GetNeighborOffsets()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };
    }

    private List<Vector2> ReconstructPath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(CellToWorld(current.position));
            current = current.parent;
        }

        path.Reverse();
        return path;
    }


    Vector2Int FindNearestWalkableCell(Vector2Int start)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (grid.walkable[current.x, current.y])
                return current;

            foreach (Vector2Int neighbor in GetNeighborsForSearch(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    void MoveAlongPath()
    {
        Vector2 targetPos = currentPath[currentTargetIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.05f)
        {
            currentTargetIndex++;

            if (currentTargetIndex >= currentPath.Count)
            {
                currentPath = null;
                Debug.Log("Путь завершён");
            }
        }
    }

    List<Vector2Int> GetNeighborsForSearch(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (neighbor.x >= 0 && neighbor.x < grid.width &&
                neighbor.y >= 0 && neighbor.y < grid.height)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        float tileWidth = 1f;
        float tileHeight = 0.5f;

        float x = worldPos.x / tileWidth;
        float y = worldPos.y / tileHeight;

        int gridX = Mathf.RoundToInt(x + y);
        int gridY = Mathf.RoundToInt(y - x);

        return new Vector2Int(gridX, gridY);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        float tileWidth = 1f;
        float tileHeight = 0.5f;

        float worldX = (cell.x - cell.y) * tileWidth * 0.5f;
        float worldY = (cell.x + cell.y) * tileHeight * 0.5f;

        return new Vector3(worldX, worldY, 0);
    }

    void Start()
    {
        walkable = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                walkable[x, y] = !Physics2D.OverlapPoint(CellToWorld(x, y), obstacleLayers);
    }

}