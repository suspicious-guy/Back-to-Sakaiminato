using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Управляет сеткой проходимости и реализует алгоритм A*.
///
/// Поддерживает два типа препятствий:
///   1. Tilemap-слои (обычные стены, пол и т.д.)
///   2. GameObject-объекты с Collider2D (ящики, NPC-барьеры и т.д.)
///
/// Повесьте этот компонент на Grid.
/// </summary>
public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    // ── Tilemap-настройки ─────────────────────────────────────────────────────

    [Header("Tilemap")]
    [Tooltip("Слой земли/пола — определяет границы мира и базово проходимые клетки.")]
    [SerializeField] private Tilemap walkableTilemap;

    [Tooltip("Tilemap-слои с препятствиями (стены, заборы и т.д.).")]
    [SerializeField] private Tilemap[] obstacleTilemaps;

    // ── GameObject-настройки ──────────────────────────────────────────────────

    [Header("GameObject-препятствия")]
    [Tooltip("Слой (Layer), на котором находятся непроходимые объекты (ящики, стены-объекты). " +
             "Создай в Project Settings → Tags and Layers слой 'Obstacle' и назначь его объектам.")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Размер проверочного бокса при сканировании клетки на наличие GameObject-коллайдера.\n" +
             "В изометрии клетка визуально — ромб ~(1.0 × 0.5), поэтому ставь (0.8, 0.35).")]
    [SerializeField] private Vector2 cellCheckSize = new Vector2(0.8f, 0.35f);

    // ── Изометрия ─────────────────────────────────────────────────────────────

    [Header("Изометрия")]
    [Tooltip("Включить диагональных соседей для A*. В изометрии — обязательно true.")]
    [SerializeField] private bool allowDiagonals = true;

    [Tooltip("false = нельзя срезать углы (диагональ заблокирована, если оба смежных соседа закрыты).\n" +
             "true  = срезание углов разрешено.")]
    [SerializeField] private bool cutCorners = false;

    // ── Отладка ───────────────────────────────────────────────────────────────

    [Header("Отладка")]
    [Tooltip("Показывать цветную сетку проходимости в окне Scene (зелёный/красный).")]
    [SerializeField] private bool showGizmos = true;

    [Tooltip("Показывать последний построенный путь A* в окне Scene (жёлтые линии).")]
    [SerializeField] private bool showPath = true;

    // ── Внутренние данные ─────────────────────────────────────────────────────

    private Dictionary<Vector3Int, bool> walkableMap = new Dictionary<Vector3Int, bool>();
    private List<Vector2> lastDebugPath; // сохраняется только для отображения в гизмо

    // ─────────────────────────────────────────────────────────────────────────
    //  Жизненный цикл
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildGrid();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Построение сетки
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Строит/перестраивает словарь проходимости.
    /// Можно вызвать повторно в рантайме, если объекты в сцене изменились
    /// (например, игрок убрал ящик).
    /// </summary>
    public void BuildGrid()
    {
        walkableMap.Clear();

        if (walkableTilemap == null)
        {
            Debug.LogError("[PathfindingGrid] walkableTilemap не назначен!");
            return;
        }

        walkableTilemap.CompressBounds();

        // ── Шаг 1: все тайлы пола → проходимые ──────────────────────────────
        foreach (Vector3Int pos in walkableTilemap.cellBounds.allPositionsWithin)
        {
            if (walkableTilemap.HasTile(pos))
                walkableMap[pos] = true;
        }

        // ── Шаг 2: Tilemap-препятствия → закрываем клетки ───────────────────
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

        // ── Шаг 3: GameObject-коллайдеры → Physics2D-сканирование ───────────
        // OverlapBox рисует невидимый прямоугольник в центре каждой клетки
        // и проверяет, попадает ли в него коллайдер из obstacleLayer.
        if (obstacleLayer != 0)
        {
            // Копируем ключи, чтобы не менять словарь во время итерации
            var keys = new List<Vector3Int>(walkableMap.Keys);
            foreach (Vector3Int pos in keys)
            {
                if (!walkableMap[pos]) continue; // уже закрыта тайлом

                Vector2 center = walkableTilemap.GetCellCenterWorld(pos);
                Collider2D hit = Physics2D.OverlapBox(center, cellCheckSize, 0f, obstacleLayer);
                if (hit != null)
                    walkableMap[pos] = false;
            }
        }

        Debug.Log($"[PathfindingGrid] Сетка построена: {walkableMap.Count} клеток.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Публичный API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Проходима ли позиция в мировых координатах?</summary>
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

    /// <summary>
    /// Строит путь A* из startWorld в goalWorld.
    /// Если цель непроходима — находит ближайшую проходимую клетку рядом.
    /// Возвращает null, если путь не существует.
    /// </summary>
    public List<Vector2> FindPath(Vector2 startWorld, Vector2 goalWorld)
    {
        Vector3Int startCell = walkableTilemap.WorldToCell(startWorld);
        Vector3Int goalCell = walkableTilemap.WorldToCell(goalWorld);

        // Если цель непроходима — ищем ближайшую проходимую через BFS
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

    // ─────────────────────────────────────────────────────────────────────────
    //  A*
    // ─────────────────────────────────────────────────────────────────────────

    // Прямые соседи (вверх/вниз/влево/вправо по сетке), стоимость 1.0
    private static readonly (Vector3Int delta, float cost)[] CardinalNeighbours =
    {
        (new Vector3Int( 1,  0, 0), 1.000f),
        (new Vector3Int(-1,  0, 0), 1.000f),
        (new Vector3Int( 0,  1, 0), 1.000f),
        (new Vector3Int( 0, -1, 0), 1.000f),
    };

    // Диагональные соседи, стоимость √2 ≈ 1.414
    private static readonly (Vector3Int delta, float cost)[] DiagonalNeighbours =
    {
        (new Vector3Int( 1,  1, 0), 1.414f),
        (new Vector3Int(-1,  1, 0), 1.414f),
        (new Vector3Int( 1, -1, 0), 1.414f),
        (new Vector3Int(-1, -1, 0), 1.414f),
    };

    private List<Vector2> AStar(Vector3Int start, Vector3Int goal)
    {
        // Приоритетная очередь: ключ = f-значение (gScore + эвристика)
        var openSet = new SortedList<float, Vector3Int>(new DuplicateKeyComparer());
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, float>();
        var inOpen = new HashSet<Vector3Int>();

        gScore[start] = 0f;
        openSet.Add(Heuristic(start, goal), start);
        inOpen.Add(start);

        int iter = 0;
        const int maxIter = 10000; // защита от зависания на огромных картах

        while (openSet.Count > 0 && iter++ < maxIter)
        {
            // Берём клетку с наименьшим f
            Vector3Int current = openSet.Values[0];
            openSet.RemoveAt(0);
            inOpen.Remove(current);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            // Обрабатываем прямых соседей
            foreach (var (delta, cost) in CardinalNeighbours)
                TryNeighbour(current, current + delta, cost, goal,
                             openSet, cameFrom, gScore, inOpen);

            // Обрабатываем диагональных соседей
            if (allowDiagonals)
            {
                foreach (var (delta, cost) in DiagonalNeighbours)
                {
                    Vector3Int nb = current + delta;

                    if (!cutCorners)
                    {
                        // Запрет срезания углов:
                        // диагональ (dx, dy) разрешена, только если хотя бы
                        // один из боковых соседей (dx,0) или (0,dy) проходим.
                        bool sideX = IsWalkableCell(current + new Vector3Int(delta.x, 0, 0));
                        bool sideY = IsWalkableCell(current + new Vector3Int(0, delta.y, 0));
                        if (!sideX && !sideY) continue;
                    }

                    TryNeighbour(current, nb, cost, goal,
                                 openSet, cameFrom, gScore, inOpen);
                }
            }
        }

        return null; // путь не найден
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

    /// <summary>
    /// Октильная эвристика — корректна для сеток с диагональным движением.
    /// При только прямых шагах автоматически деградирует до манхэттена.
    /// </summary>
    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        if (allowDiagonals)
            return (dx + dy) + (1.414f - 2f) * Mathf.Min(dx, dy);
        return dx + dy;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Вспомогательные
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// BFS вширь от target — ищет ближайшую проходимую клетку.
    /// Используется, когда игрок кликнул на непроходимое место.
    /// </summary>
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

            if (visited.Count > 300) break; // если всё вокруг закрыто
        }
        return fallback;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Гизмо (только в Unity Editor, в билде не существует)
    // ─────────────────────────────────────────────────────────────────────────

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
        // ── Сетка проходимости ────────────────────────────────────────────────
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

                // Четыре вершины ромба в изометрии:
                //   верхняя, правая, нижняя, левая
                Vector3 top   = c + new Vector3( 0f,  hh, 0f);
                Vector3 right = c + new Vector3( hw,  0f, 0f);
                Vector3 bot   = c + new Vector3( 0f, -hh, 0f);
                Vector3 left  = c + new Vector3(-hw,  0f, 0f);

                // Заливка: два треугольника покрывают ромб
                Gizmos.color = fillColor;
                DrawFilledDiamond(top, right, bot, left);

                // Контур: четыре ребра
                Gizmos.color = borderColor;
                Gizmos.DrawLine(top, right);
                Gizmos.DrawLine(right, bot);
                Gizmos.DrawLine(bot, left);
                Gizmos.DrawLine(left, top);
            }
        }

        // ── Последний путь A* ─────────────────────────────────────────────────
        if (showPath && lastDebugPath != null && lastDebugPath.Count > 1)
        {
            float dotR = gizmoDiamondHalfH * 0.35f; // размер точек пропорционален клетке

            Gizmos.color = new Color(0.94f, 0.62f, 0.15f, 0.9f); // янтарный
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

    /// <summary>
    /// Рисует закрашенный ромб через два треугольника.
    /// Unity Gizmos не умеет рисовать произвольные полигоны напрямую —
    /// поэтому используем DrawMesh с временным мешем.
    /// </summary>
    private static readonly Mesh _diamondMesh = CreateDiamondMesh();

    private static Mesh CreateDiamondMesh()
    {
        // Единичный ромб (вершины в локальном пространстве).
        // Масштаб применяется через Gizmos.matrix при вызове DrawMesh.
        var mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3( 0f,  1f, 0f), // top    [0]
            new Vector3( 1f,  0f, 0f), // right  [1]
            new Vector3( 0f, -1f, 0f), // bottom [2]
            new Vector3(-1f,  0f, 0f), // left   [3]
        };
        // Два треугольника: top-right-bottom и top-bottom-left
        mesh.triangles = new int[] { 0, 1, 2,  0, 2, 3 };
        mesh.RecalculateNormals();
        return mesh;
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

    // ─────────────────────────────────────────────────────────────────────────
    //  Вспомогательный класс
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// SortedList не допускает дублирующиеся ключи.
    /// Этот компаратор считает равные float-ключи "разными" (возвращает 1),
    /// что позволяет хранить несколько клеток с одинаковым f-значением.
    /// </summary>
    private class DuplicateKeyComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            int r = x.CompareTo(y);
            return r == 0 ? 1 : r;
        }
    }
}