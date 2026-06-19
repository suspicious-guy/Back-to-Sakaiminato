using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Управляет сеткой проходимости и реализует алгоритм A*.
/// Повесьте этот компонент на любой GameObject в сцене (например, на Grid).
/// Укажите в инспекторе все Tilemap-слои, которые считаются непроходимыми.
/// </summary>
public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    [Header("Tilemap настройки")]
    [Tooltip("Tilemap, по которому ходит игрок (земля/пол). Нужен для определения границ мира.")]
    [SerializeField] private Tilemap walkableTilemap;

    [Tooltip("Все Tilemap-слои, которые являются препятствиями (стены, объекты и т.д.)")]
    [SerializeField] private Tilemap[] obstacleTilemaps;

    [Header("Дополнительно")]
    [Tooltip("Показывать отладочные гизмо в редакторе")]
    [SerializeField] private bool showGizmos = true;

    // Внутренняя сетка: ключ — позиция клетки, значение — проходима ли она
    private Dictionary<Vector3Int, bool> walkableMap = new Dictionary<Vector3Int, bool>();
    private BoundsInt gridBounds;

    // ─── Жизненный цикл ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildGrid();
    }

    // ─── Построение сетки ─────────────────────────────────────────────────────

    /// <summary>Строит словарь проходимых клеток из тайлмапов.</summary>
    public void BuildGrid()
    {
        walkableMap.Clear();

        if (walkableTilemap == null)
        {
            Debug.LogError("[PathfindingGrid] walkableTilemap не назначен!");
            return;
        }

        walkableTilemap.CompressBounds();
        gridBounds = walkableTilemap.cellBounds;

        // Все клетки проходимого слоя изначально — проходимые
        foreach (Vector3Int pos in gridBounds.allPositionsWithin)
        {
            if (walkableTilemap.HasTile(pos))
                walkableMap[pos] = true;
        }

        // Закрываем клетки, занятые препятствиями
        if (obstacleTilemaps != null)
        {
            foreach (Tilemap obstacle in obstacleTilemaps)
            {
                if (obstacle == null) continue;
                obstacle.CompressBounds();
                foreach (Vector3Int pos in obstacle.cellBounds.allPositionsWithin)
                {
                    if (obstacle.HasTile(pos))
                        walkableMap[pos] = false;
                }
            }
        }

        Debug.Log($"[PathfindingGrid] Сетка построена: {walkableMap.Count} клеток.");
    }

    // ─── Публичный API ────────────────────────────────────────────────────────

    /// <summary>Проходима ли клетка в мировых координатах?</summary>
    public bool IsWalkable(Vector2 worldPos)
    {
        Vector3Int cell = walkableTilemap.WorldToCell(worldPos);
        return walkableMap.TryGetValue(cell, out bool w) && w;
    }

    /// <summary>Проходима ли клетка в координатах сетки?</summary>
    public bool IsWalkableCell(Vector3Int cell)
    {
        return walkableMap.TryGetValue(cell, out bool w) && w;
    }

    /// <summary>Переводит мировые координаты в координаты клетки.</summary>
    public Vector3Int WorldToCell(Vector2 worldPos)
        => walkableTilemap.WorldToCell(worldPos);

    /// <summary>Переводит координаты клетки в центр клетки в мировом пространстве.</summary>
    public Vector2 CellToWorld(Vector3Int cell)
        => walkableTilemap.GetCellCenterWorld(cell);

    /// <summary>
    /// Строит путь алгоритмом A* от startWorld до goalWorld.
    /// Возвращает список мировых позиций (центры клеток) или null, если путь не найден.
    /// Если цель непроходима — ищет ближайшую проходимую клетку.
    /// </summary>
    public List<Vector2> FindPath(Vector2 startWorld, Vector2 goalWorld)
    {
        Vector3Int startCell = walkableTilemap.WorldToCell(startWorld);
        Vector3Int goalCell = walkableTilemap.WorldToCell(goalWorld);

        // Если цель непроходима — берём ближайшую проходимую клетку
        if (!IsWalkableCell(goalCell))
        {
            goalCell = FindNearestWalkable(goalCell, startCell);
            if (goalCell == startCell) return null; // уже стоим там
        }

        if (!IsWalkableCell(startCell)) return null;
        if (startCell == goalCell) return null;

        return AStar(startCell, goalCell);
    }

    // ─── A* ───────────────────────────────────────────────────────────────────

    private static readonly Vector3Int[] Neighbours = {
        new Vector3Int( 1,  0, 0), new Vector3Int(-1,  0, 0),
        new Vector3Int( 0,  1, 0), new Vector3Int( 0, -1, 0),
        // диагонали (раскомментируй если нужны)
        // new Vector3Int( 1,  1, 0), new Vector3Int(-1,  1, 0),
        // new Vector3Int( 1, -1, 0), new Vector3Int(-1, -1, 0),
    };

    private List<Vector2> AStar(Vector3Int start, Vector3Int goal)
    {
        // Приоритетная очередь через SortedList (минимальная куча)
        var openSet = new SortedList<float, Vector3Int>(new DuplicateKeyComparer());
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, float>();
        var fScore = new Dictionary<Vector3Int, float>();
        var inOpen = new HashSet<Vector3Int>();

        gScore[start] = 0f;
        float h0 = Heuristic(start, goal);
        fScore[start] = h0;
        openSet.Add(h0, start);
        inOpen.Add(start);

        int iterations = 0;
        const int maxIterations = 10000;

        while (openSet.Count > 0 && iterations++ < maxIterations)
        {
            Vector3Int current = openSet.Values[0];
            openSet.RemoveAt(0);
            inOpen.Remove(current);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (Vector3Int delta in Neighbours)
            {
                Vector3Int neighbour = current + delta;
                if (!IsWalkableCell(neighbour)) continue;

                float tentativeG = gScore.GetValueOrDefault(current, float.MaxValue) + 1f;

                if (tentativeG < gScore.GetValueOrDefault(neighbour, float.MaxValue))
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeG;
                    float f = tentativeG + Heuristic(neighbour, goal);
                    fScore[neighbour] = f;

                    if (!inOpen.Contains(neighbour))
                    {
                        openSet.Add(f, neighbour);
                        inOpen.Add(neighbour);
                    }
                }
            }
        }

        return null; // путь не найден
    }

    private List<Vector2> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
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

    private static float Heuristic(Vector3Int a, Vector3Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // манхэттен

    // ─── Вспомогательные ──────────────────────────────────────────────────────

    /// <summary>BFS-поиск ближайшей проходимой клетки к target (от startHint).</summary>
    private Vector3Int FindNearestWalkable(Vector3Int target, Vector3Int startHint)
    {
        var visited = new HashSet<Vector3Int> { target };
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(target);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (IsWalkableCell(current)) return current;

            foreach (Vector3Int delta in Neighbours)
            {
                Vector3Int next = current + delta;
                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }

            if (visited.Count > 200) break; // ограничение поиска
        }

        return startHint;
    }

    // ─── Гизмо ────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || walkableMap == null) return;
        foreach (var kvp in walkableMap)
        {
            Gizmos.color = kvp.Value
                ? new Color(0f, 1f, 0f, 0.15f)
                : new Color(1f, 0f, 0f, 0.25f);
            Vector3 center = walkableTilemap != null
                ? walkableTilemap.GetCellCenterWorld(kvp.Key)
                : (Vector3)(Vector2)kvp.Key;
            Gizmos.DrawCube(center, Vector3.one * 0.45f);
        }
    }
#endif

    // ─── Вспомогательный класс ────────────────────────────────────────────────

    /// <summary>Компаратор для SortedList, разрешающий дублирующиеся ключи.</summary>
    private class DuplicateKeyComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            int result = x.CompareTo(y);
            return result == 0 ? 1 : result; // одинаковые ключи считаем "больше"
        }
    }
}