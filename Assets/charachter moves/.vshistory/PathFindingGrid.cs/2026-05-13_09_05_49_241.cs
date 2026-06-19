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
    }

    private void Start()
    {
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
            int closedByGameObject = 0;

            var keys = new List<Vector3Int>(walkableMap.Keys);
            foreach (Vector3Int pos in keys)
            {
                if (!walkableMap[pos]) continue;

                Vector2 c = walkableTilemap.GetCellCenterWorld(pos);
                float hw = cellCheckSize.x * 0.5f;
                float hh = cellCheckSize.y * 0.5f;

                Vector2[] probePoints = {
                    c,
                    c + new Vector2( hw * 0.6f,  0f),
                    c + new Vector2(-hw * 0.6f,  0f),
                    c + new Vector2( 0f,  hh * 0.6f),
                    c + new Vector2( 0f, -hh * 0.6f),
                };

                bool blocked = false;
                Collider2D blocker = null;
                foreach (Vector2 probe in probePoints)
                {
                    Collider2D hit = Physics2D.OverlapPoint(probe, obstacleLayer);
                    if (hit != null) { blocked = true; blocker = hit; break; }
                }

                if (blocked)
                {
                    walkableMap[pos] = false;
                    closedByGameObject++;
                    if (showGizmos)
                        Debug.Log($"[PathfindingGrid] Клетка {pos} закрыта: '{blocker.gameObject.name}'");
                }
            }

            Debug.Log($"[PathfindingGrid] GameObject-препятствия закрыли {closedByGameObject} клеток.");
        }
        else
        {
            Debug.LogWarning("[PathfindingGrid] obstacleLayer == 0 (Nothing). " +
                             "GameObject-препятствия не будут учтены. " +
                             "Назначь слой в инспекторе PathfindingGrid.");
        }

        int walkable = 0;
        int blockedCount = 0;
        foreach (var v in walkableMap.Values) { if (v) walkable++; else blockedCount++; }
        Debug.Log($"[PathfindingGrid] Сетка построена: {walkableMap.Count} клеток " +
                  $"({walkable} проходимых, {blockedCount} заблокированных).");
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
        lastDebugPath = path; // сохраняем для отрисовки в гизмо
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


    [Header("Размер ромбов гизмо")]
    [Tooltip("Половина ширины ромба в мировых единицах. " +
             "По умолчанию = половина ширины изометрической клетки вашего тайлмапа.\n" +
             "Увеличь, если ромбы кажутся маленькими.")]
    [SerializeField] private float gizmoDiamondHalfW = 0.5f;

    [Tooltip("Половина высоты ромба. Обычно = gizmoDiamondHalfW / 2 для изометрии 2:1.")]
    [SerializeField] private float gizmoDiamondHalfH = 0.25f;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmos && walkableMap != null && walkableTilemap != null)
        {
            foreach (var kvp in walkableMap)
            {
                Color fillColor = kvp.Value
                    ? new Color(0.11f, 0.62f, 0.46f, 0.18f)  // зелёный = проходимо
                    : new Color(0.85f, 0.35f, 0.19f, 0.32f); // красный = препятствие

                Color borderColor = kvp.Value
                    ? new Color(0.11f, 0.62f, 0.46f, 0.7f)
                    : new Color(0.85f, 0.35f, 0.19f, 0.85f);

                Vector3 c = walkableTilemap.GetCellCenterWorld(kvp.Key);
                float hw = gizmoDiamondHalfW;
                float hh = gizmoDiamondHalfH;

                Vector3 top   = c + new Vector3( 0f,  hh, 0f);
                Vector3 right = c + new Vector3( hw,  0f, 0f);
                Vector3 bot   = c + new Vector3( 0f, -hh, 0f);
                Vector3 left  = c + new Vector3(-hw,  0f, 0f);

                Gizmos.color = fillColor;
                DrawFilledDiamond(top, right, bot, left);

                Gizmos.color = borderColor;
                Gizmos.DrawLine(top, right);
                Gizmos.DrawLine(right, bot);
                Gizmos.DrawLine(bot, left);
                Gizmos.DrawLine(left, top);
            }
        }

        if (showPath && lastDebugPath != null && lastDebugPath.Count > 1)
        {
            float dotR = gizmoDiamondHalfH * 0.35f;

            Gizmos.color = new Color(0.94f, 0.62f, 0.15f, 0.9f);
            for (int i = 0; i < lastDebugPath.Count - 1; i++)
            {
                Gizmos.DrawLine(lastDebugPath[i], lastDebugPath[i + 1]);
                Gizmos.DrawSphere(lastDebugPath[i], dotR);
            }

            // Конечная точка — голубым и крупнее
            Gizmos.color = new Color(0.12f, 0.55f, 0.84f, 0.95f);
            Gizmos.DrawSphere(lastDebugPath[lastDebugPath.Count - 1], dotR * 1.6f);
        }
    }

        private Mesh _diamondMesh; // lazy-init в первом вызове OnDrawGizmos
 
    private Mesh GetDiamondMesh()
    {
        if (_diamondMesh != null) return _diamondMesh;
 
        // Единичный ромб (вершины в локальном пространстве).
        // Масштаб применяется через Gizmos.matrix при вызове DrawMesh.
        _diamondMesh = new Mesh();
        _diamondMesh.vertices = new Vector3[]
        {
            new Vector3( 0f,  1f, 0f), // top    [0]
            new Vector3( 1f,  0f, 0f), // right  [1]
            new Vector3( 0f, -1f, 0f), // bottom [2]
            new Vector3(-1f,  0f, 0f), // left   [3]
        };
        // Два треугольника: top-right-bottom и top-bottom-left
        _diamondMesh.triangles = new int[] { 0, 1, 2,  0, 2, 3 };
        _diamondMesh.RecalculateNormals();
        return _diamondMesh;
    }

    private void DrawFilledDiamond(Vector3 top, Vector3 right, Vector3 bot, Vector3 left)
    {
        // Восстанавливаем центр и полуоси из четырёх вершин
        Vector3 center = (top + bot) * 0.5f;
        float hw = Vector3.Distance(center, right);
        float hh = Vector3.Distance(center, top);

        // Сохраняем текущую матрицу и задаём масштаб под нужный размер ромба
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, new Vector3(hw, hh, 0.001f));
        Gizmos.DrawMesh(_diamondMesh);
        Gizmos.matrix = prev;
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