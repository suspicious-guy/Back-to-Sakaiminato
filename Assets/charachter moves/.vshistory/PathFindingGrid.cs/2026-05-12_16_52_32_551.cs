using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    [Header("Tilemap")]
    [Tooltip("Слой земли/пола — определяет границы мира и базово проходимые клетки.")]
    [SerializeField] private Tilemap walkableTilemap;

    [Tooltip("Tilemap-слои с препятствиями (стены, заборы и т.д.).")]
    [SerializeField] private Tilemap[] obstacleTilemaps;


    [Header("GameObject-препятствия")]
    [Tooltip("Слой (Layer), на котором находятся непроходимые объекты (ящики, стены-объекты). " +
             "Создай в Project Settings → Tags and Layers слой 'Obstacle' и назначь его объектам.")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Размер проверочного бокса при сканировании клетки на наличие GameObject-коллайдера.\n" +
             "В изометрии клетка визуально — ромб ~(1.0 × 0.5), поэтому ставь (0.8, 0.35).")]
    [SerializeField] private Vector2 cellCheckSize = new Vector2(0.8f, 0.35f);


    [Header("Изометрия")]
    [Tooltip("Включить диагональных соседей для A*. В изометрии — обязательно true.")]
    [SerializeField] private bool allowDiagonals = true;

    [Tooltip("false = нельзя срезать углы (диагональ заблокирована, если оба смежных соседа закрыты).\n" +
             "true  = срезание углов разрешено.")]
    [SerializeField] private bool cutCorners = false;


    [Header("Отладка")]
    [Tooltip("Показывать цветную сетку проходимости в окне Scene (зелёный/красный).")]
    [SerializeField] private bool showGizmos = true;

    [Tooltip("Показывать последний построенный путь A* в окне Scene (жёлтые линии).")]
    [SerializeField] private bool showPath = true;


    private Dictionary<Vector3Int, bool> walkableMap = new Dictionary<Vector3Int, bool>();
    private List<Vector2> lastDebugPath;


    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildGrid();
    }

    public void BuildGrid()
    {
        walkableMap.Clear();

        if (walkableTilemap == null)
        {
            Debug.LogError("[PathfindingGrid] walkableTilemap не назначен!");
            return;
        }

        walkableTilemap.CompressBounds();

        foreach (Vector3Int pos in walkableTilemap.cellBounds.allPositionsWithin)
        {
            if (walkableTilemap.HasTile(pos))
                walkableMap[pos] = true;
        }

        if (obstacleTilemaps != null)
        {
            foreach (Tilemap obs in obstacleTilemaps)
            {
                if (obs == null) continue;
                obs.CompressBounds();
                foreach (Vector3Int pos in obs.cellBounds.allPositionsWithin)
                {
                    if (obs.HasTile(pos))
                        walkableMap[pos] = false;
                }
            }
        }

        if (obstacleLayer != 0)
        {
            var keys = new List<Vector3Int>(walkableMap.Keys);
            foreach (Vector3Int pos in keys)
            {
                if (!walkableMap[pos]) continue;

                Vector2 center = walkableTilemap.GetCellCenterWorld(pos);
                Collider2D hit = Physics2D.OverlapBox(center, cellCheckSize, 0f, obstacleLayer);
                if (hit != null)
                    walkableMap[pos] = false;
            }
        }

        Debug.Log($"[PathfindingGrid] Сетка построена: {walkableMap.Count} клеток.");
    }

    public bool IsWalkable(Vector2 worldPos)
    {
        Vector3Int cell = walkableTilemap.WorldToCell(worldPos);
        return walkableMap.TryGetValue(cell, out bool w) && w;
    }

    public bool IsWalkableCell(Vector3Int cell)
        => walkableMap.TryGetValue(cell, out bool w) && w;

    public Vector3Int WorldToCell(Vector2 worldPos)
        => walkableTilemap.WorldToCell(worldPos);

    public Vector2 CellToWorld(Vector3Int cell)
        => walkableTilemap.GetCellCenterWorld(cell);

    public List<Vector2> FindPath(Vector2 startWorld, Vector2 goalWorld)
    {
        Vector3Int startCell = walkableTilemap.WorldToCell(startWorld);
        Vector3Int goalCell = walkableTilemap.WorldToCell(goalWorld);

        if (!IsWalkableCell(goalCell))
            goalCell = FindNearestWalkable(goalCell, startCell);

        if (!IsWalkableCell(startCell) || startCell == goalCell)
            return null;

        List<Vector2> path = AStar(startCell, goalCell);

#if UNITY_EDITOR
        lastDebugPath = path;
#endif
        return path;
    }

    private static readonly (Vector3Int delta, float cost)[] CardinalNeighbours =
    {
        (new Vector3Int( 1,  0, 0), 1.000f),
        (new Vector3Int(-1,  0, 0), 1.000f),
        (new Vector3Int( 0,  1, 0), 1.000f),
        (new Vector3Int( 0, -1, 0), 1.000f),
    };

    private static readonly (Vector3Int delta, float cost)[] DiagonalNeighbours =
    {
        (new Vector3Int( 1,  1, 0), 1.414f),
        (new Vector3Int(-1,  1, 0), 1.414f),
        (new Vector3Int( 1, -1, 0), 1.414f),
        (new Vector3Int(-1, -1, 0), 1.414f),
    };

    private List<Vector2> AStar(Vector3Int start, Vector3Int goal)
    {
        var openSet = new SortedList<float, Vector3Int>(new DuplicateKeyComparer());
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, float>();
        var inOpen = new HashSet<Vector3Int>();

        gScore[start] = 0f;
        openSet.Add(Heuristic(start, goal), start);
        inOpen.Add(start);

        int iter = 0;
        const int maxIter = 10000;

        while (openSet.Count > 0 && iter++ < maxIter)
        {
            Vector3Int current = openSet.Values[0];
            openSet.RemoveAt(0);
            inOpen.Remove(current);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var (delta, cost) in CardinalNeighbours)
                TryNeighbour(current, current + delta, cost, goal,
                             openSet, cameFrom, gScore, inOpen);

            if (allowDiagonals)
            {
                foreach (var (delta, cost) in DiagonalNeighbours)
                {
                    Vector3Int nb = current + delta;

                    if (!cutCorners)
                    {

                        bool sideX = IsWalkableCell(current + new Vector3Int(delta.x, 0, 0));
                        bool sideY = IsWalkableCell(current + new Vector3Int(0, delta.y, 0));
                        if (!sideX && !sideY) continue;
                    }

                    TryNeighbour(current, nb, cost, goal,
                                 openSet, cameFrom, gScore, inOpen);
                }
            }
        }

        return null;
    }

    private void TryNeighbour(
        Vector3Int current,
        Vector3Int neighbour,
        float moveCost,
        Vector3Int goal,
        SortedList<float, Vector3Int> openSet,
        Dictionary<Vector3Int, Vector3Int> cameFrom,
        Dictionary<Vector3Int, float> gScore,
        HashSet<Vector3Int> inOpen)
    {
        if (!IsWalkableCell(neighbour)) return;

        float tentativeG = gScore.GetValueOrDefault(current, float.MaxValue) + moveCost;

        if (tentativeG < gScore.GetValueOrDefault(neighbour, float.MaxValue))
        {
            cameFrom[neighbour] = current;
            gScore[neighbour] = tentativeG;
            float f = tentativeG + Heuristic(neighbour, goal);

            if (!inOpen.Contains(neighbour))
            {
                openSet.Add(f, neighbour);
                inOpen.Add(neighbour);
            }
        }
    }

    private List<Vector2> ReconstructPath(
        Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        var path = new List<Vector2>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(CellToWorld(current));
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        if (allowDiagonals)
            return (dx + dy) + (1.414f - 2f) * Mathf.Min(dx, dy);
        return dx + dy;
    }

    private Vector3Int FindNearestWalkable(Vector3Int target, Vector3Int fallback)
    {
        var visited = new HashSet<Vector3Int> { target };
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(target);

        while (queue.Count > 0)
        {
            Vector3Int cur = queue.Dequeue();
            if (IsWalkableCell(cur)) return cur;

            foreach (var (delta, _) in CardinalNeighbours)
            {
                Vector3Int next = cur + delta;
                if (visited.Add(next))
                    queue.Enqueue(next);
            }

            if (visited.Count > 300) break;
        }
        return fallback;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmos && walkableMap != null && walkableTilemap != null)
        {
            foreach (var kvp in walkableMap)
            {
                Gizmos.color = kvp.Value
                    ? new Color(0f, 1f, 0f, 0.15f)
                    : new Color(1f, 0f, 0f, 0.30f);

                Vector3 center = walkableTilemap.GetCellCenterWorld(kvp.Key);

                Gizmos.DrawCube(center, new Vector3(0.45f, 0.22f, 0f));
            }
        }

        if (showPath && lastDebugPath != null && lastDebugPath.Count > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < lastDebugPath.Count - 1; i++)
            {
                Gizmos.DrawLine(lastDebugPath[i], lastDebugPath[i + 1]);
                Gizmos.DrawSphere(lastDebugPath[i], 0.05f);
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lastDebugPath[lastDebugPath.Count - 1], 0.08f);
        }
    }
#endif

    private class DuplicateKeyComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            int r = x.CompareTo(y);
            return r == 0 ? 1 : r;
        }
    }
}