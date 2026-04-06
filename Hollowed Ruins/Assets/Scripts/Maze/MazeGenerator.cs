using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator Instance { get; private set; }

    [Header("Maze Settings")]
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 7;
    [SerializeField] private float cellSize = 4f;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject keyPiecePrefab;
    [SerializeField] private GameObject exitPrefab;

    [Header("Piece Settings")]
    [SerializeField] private int keyPieceCount = 5;

    // 0 = wall, 1 = corridor
    private int[,] _grid;

    private Vector2Int _playerSpawn;
    private Vector2Int _ghostSpawn;
    private Vector2Int _exitCell;
    private List<Vector2Int> _keyPieceCells = new();
    private List<GameObject> _spawnedObjects = new();
    private GameObject _exitObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GenerateMaze();
        BuildMesh();
        PlaceObjects();
    }

    // ─── Maze Generation (Recursive Backtracking) ────────────────────────────

    void GenerateMaze()
    {
        _grid = new int[width, height];

        // Start carving from (0,0)
        CarveFrom(0, 0);
    }

    void CarveFrom(int x, int y)
    {
        _grid[x, y] = 1;

        List<Vector2Int> directions = new()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Shuffle(directions);

        foreach (var dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            if (InBounds(nx, ny) && _grid[nx, ny] == 0)
            {
                // Knock down the wall between current and neighbor
                _grid[x + dir.x, y + dir.y] = 1;
                CarveFrom(nx, ny);
            }
        }
    }

    // ─── Mesh Building ────────────────────────────────────────────────────────

    void BuildMesh()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = CellToWorld(x, y);

                if (_grid[x, y] == 1)
                {
                    Spawn(floorPrefab, pos);
                }
                else
                {
                    Spawn(wallPrefab, pos);
                }
            }
        }
    }

    // ─── Object Placement ─────────────────────────────────────────────────────

    void PlaceObjects()
    {
        List<Vector2Int> corridors = GetAllCorridors();
        Shuffle(corridors);

        int idx = 0;

        // Player spawn
        _playerSpawn = corridors[idx++];

        // Ghost spawn — keep it far from player
        _ghostSpawn = corridors[corridors.Count - 1];

        // Exit cell
        _exitCell = corridors[corridors.Count - 2];
        _exitObject = Spawn(exitPrefab, CellToWorld(_exitCell.x, _exitCell.y));
        if (_exitObject != null) _exitObject.SetActive(false); // hidden until all pieces collected

        // Key pieces
        _keyPieceCells.Clear();
        for (int i = 0; i < keyPieceCount && idx < corridors.Count - 2; i++, idx++)
        {
            _keyPieceCells.Add(corridors[idx]);
            Spawn(keyPiecePrefab, CellToWorld(corridors[idx].x, corridors[idx].y));
        }
    }

    public void RevealExit()
    {
        if (_exitObject != null)
            _exitObject.SetActive(true);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    public Vector3 GetPlayerSpawnWorld() => CellToWorld(_playerSpawn.x, _playerSpawn.y);
    public Vector3 GetGhostSpawnWorld()  => CellToWorld(_ghostSpawn.x,  _ghostSpawn.y);

    Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(x * cellSize, 0f, y * cellSize);
    }

    List<Vector2Int> GetAllCorridors()
    {
        var list = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (_grid[x, y] == 1)
                    list.Add(new Vector2Int(x, y));
        return list;
    }

    bool InBounds(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

    GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        var obj = Instantiate(prefab, position, Quaternion.identity, transform);
        _spawnedObjects.Add(obj);
        return obj;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
