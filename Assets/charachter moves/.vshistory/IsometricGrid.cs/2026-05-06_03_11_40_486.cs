using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IsometricGrid : MonoBehaviour
{
    [Header("Размеры сетки")]
    public int width = 50;
    public int height = 50;

    [Header("Размер тайла в юнитах")]
    public float tileWidth = 1f;
    public float tileHeight = 0.5f;

    [Header("Препятствия (через Sorting Layer)")]
    public string obstacleSortingLayer = "Obstacles"; // имя Sorting Layer
    public bool useSortingLayerInsteadOfPhysics = true; // если true — используем сортировку, если false — физику

    [Header("Препятствия (через LayerMask)")]
    public LayerMask obstacleLayers;

    [Header("Визуализация")]
    public bool showGizmos = true;
    public Color walkableColor = Color.green;
    public Color blockedColor = Color.red;

    public bool[,] walkable;

    // Кэш для всех объектов на сцене с нужным Sorting Layer
    private HashSet<GameObject> obstacleObjects = new HashSet<GameObject>();

    private void Awake()
    {
        if (useSortingLayerInsteadOfPhysics)
            CacheObstaclesBySortingLayer();

        BuildGrid();
    }

    // Кэшируем все объекты с указанным Sorting Layer
    private void CacheObstaclesBySortingLayer()
    {
        obstacleObjects.Clear();

        // Находим все объекты с Renderer (спрайты, тайлы)
        SpriteRenderer[] allRenderers = FindObjectsOfType<SpriteRenderer>();

        foreach (SpriteRenderer renderer in allRenderers)
        {
            // Проверяем Sorting Layer ID (а не имя, для производительности)
            int targetLayerID = SortingLayer.NameToID(obstacleSortingLayer);
            if (renderer.sortingLayerID == targetLayerID)
            {
                obstacleObjects.Add(renderer.gameObject);
            }
        }

        Debug.Log($"Найдено {obstacleObjects.Count} объектов с Sorting Layer '{obstacleSortingLayer}'");
    }

    // Построение сетки
    public void BuildGrid()
    {
        walkable = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = CellToWorld(new Vector2Int(x, y));
                bool isBlocked = false;

                if (useSortingLayerInsteadOfPhysics)
                {
                    isBlocked = IsBlockedBySortingLayer(worldPos);
                }
                else
                {
                    Collider2D hit = Physics2D.OverlapPoint(worldPos, obstacleLayers);
                    isBlocked = (hit != null);
                }

                walkable[x, y] = !isBlocked;
            }
        }
    }

    // Проверка, есть ли препятствие с нужным Sorting Layer в точке
    private bool IsBlockedBySortingLayer(Vector3 worldPos)
    {
        foreach (GameObject obstacle in obstacleObjects)
        {
            if (obstacle == null) continue;

            // Проверяем коллайдер (если есть)
            Collider2D col = obstacle.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(worldPos))
                return true;

            // Если коллайдера нет — проверяем по границам спрайта
            SpriteRenderer renderer = obstacle.GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null)
            {
                Bounds bounds = renderer.bounds;
                if (bounds.Contains(worldPos))
                    return true;
            }
        }

        return false;
    }

    // Перестроить сетку вручную (вызывать, если изменились препятствия)
    public void RefreshGrid()
    {
        if (useSortingLayerInsteadOfPhysics)
            CacheObstaclesBySortingLayer();

        BuildGrid();
    }

    // ========== ПРЕОБРАЗОВАНИЯ КООРДИНАТ ==========

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        float x = worldPos.x / tileWidth;
        float y = worldPos.y / tileHeight;

        int gridX = Mathf.RoundToInt(x + y);
        int gridY = Mathf.RoundToInt(y - x);

        return new Vector2Int(gridX, gridY);
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        float worldX = (cell.x - cell.y) * tileWidth * 0.5f;
        float worldY = (cell.x + cell.y) * tileHeight * 0.5f;

        return new Vector2(worldX, worldY);
    }

    public bool IsWalkable(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return false;
        return walkable[cell.x, cell.y];
    }

    // ========== A* АЛГОРИТМ ==========

    public List<Vector2> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector2Int startCell = WorldToCell(startWorld);
        Vector2Int targetCell = WorldToCell(targetWorld);

        if (!IsWalkable(startCell) || !IsWalkable(targetCell))
            return null;

        var openSet = new List<Node>();
        var closedSet = new HashSet<Vector2Int>();

        Node startNode = new Node(startCell, null, 0, Heuristic(startCell, targetCell));
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet.OrderBy(n => n.FCost).First();

            if (current.position == targetCell)
                return ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current.position);

            foreach (Vector2Int neighborOffset in GetNeighborOffsets())
            {
                Vector2Int neighborPos = current.position + neighborOffset;

                if (!IsWalkable(neighborPos))
                    continue;
                if (closedSet.Contains(neighborPos))
                    continue;

                float tentativeGCost = current.GCost + Vector2Int.Distance(current.position, neighborPos);
                Node neighborNode = openSet.FirstOrDefault(n => n.position == neighborPos);

                if (neighborNode == null)
                {
                    float hCost = Heuristic(neighborPos, targetCell);
                    Node newNode = new Node(neighborPos, current, tentativeGCost, hCost);
                    openSet.Add(newNode);
                }
                else if (tentativeGCost < neighborNode.GCost)
                {
                    neighborNode.parent = current;
                    neighborNode.GCost = tentativeGCost;
                    neighborNode.HCost = Heuristic(neighborPos, targetCell);
                }
            }
        }

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

    public Vector2Int FindNearestWalkable(Vector2Int start)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (IsWalkable(current))
                return current;

            foreach (Vector2Int offset in GetNeighborOffsets())
            {
                Vector2Int neighbor = current + offset;
                if (!visited.Contains(neighbor) &&
                    neighbor.x >= 0 && neighbor.x < width &&
                    neighbor.y >= 0 && neighbor.y < height)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public float GCost;
        public float HCost;
        public float FCost => GCost + HCost;

        public Node(Vector2Int pos, Node par, float g, float h)
        {
            position = pos;
            parent = par;
            GCost = g;
            HCost = h;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (walkable == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = CellToWorld(new Vector2Int(x, y));
                Gizmos.color = (walkable[x, y]) ? walkableColor : blockedColor;
                Gizmos.DrawWireCube(worldPos, new Vector3(tileWidth, tileHeight, 0));
            }
        }
    }
}