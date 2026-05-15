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

    [Header("Препятствия")]
    public LayerMask obstacleLayers;

    [Header("Визуализация (опционально)")]
    public bool showGizmos = true;
    public Color walkableColor = Color.green;
    public Color blockedColor = Color.red;


    public bool[,] walkable;

    private void Awake()
    {
        BuildGrid();
    }

    // Построение сетки: сканируем каждый тайл на наличие препятствий
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

    // Проверка, проходима ли клетка
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

    // Эвристика (манхэттенское расстояние для сетки)
    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Соседние клетки (4 направления, без диагоналей)
    private List<Vector2Int> GetNeighborOffsets()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 1),  // вверх
            new Vector2Int(0, -1), // вниз
            new Vector2Int(-1, 0), // влево
            new Vector2Int(1, 0)   // вправо
        };
    }

    // Восстановление пути из узлов
    private List<Vector2> ReconstructPath(Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(CellToWorld(current.position));
            current = current.parent;
        }

        path.Reverse(); // от старта к цели
        return path;
    }

    // ========== BFS ДЛЯ ПОИСКА БЛИЖАЙШЕЙ ДОСТУПНОЙ КЛЕТКИ ==========

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

    // ========== ВСПОМОГАТЕЛЬНЫЙ КЛАСС ==========

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

    // ========== ВИЗУАЛИЗАЦИЯ В РЕДАКТОРЕ ==========

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (walkable == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = CellToWorld(new Vector2Int(x, y));
                Gizmos.color = walkable[x, y] ? walkableColor : blockedColor;
                Gizmos.DrawWireCube(worldPos, new Vector3(tileWidth, tileHeight, 0));
            }
        }
    }
}