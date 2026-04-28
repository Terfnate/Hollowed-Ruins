using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator Instance { get; private set; }

    [Header("Maze Settings")]
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 7;
    [SerializeField] private float cellSize = 4f;
    [SerializeField] private float ceilingHeight = 20f;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject keyPiecePrefab;
    [SerializeField] private GameObject exitPrefab;

    [Header("Piece Settings")]
    [SerializeField] private int keyPieceCount = 5;

    [Header("References")]
    [SerializeField] private GhostAI ghost;
    [SerializeField] private Transform player;

    private int[,] _grid;
    private Vector2Int _playerSpawn;
    private Vector2Int _ghostSpawn;
    private Vector2Int _exitCell;
    private readonly List<Vector2Int> _keyPieceCells = new();
    private readonly List<GameObject> _spawnedObjects = new();
    private GameObject _exitObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GenerateMaze();
        BuildMesh();
        BakeNavMesh();
        PlaceObjects();
        PositionActors();
        GetComponent<MinimapFog>()?.OnMazeReady(width, height, cellSize, _playerSpawn);
    }

    void BakeNavMesh()
    {
        var surface = GetComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry    = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
    }

    void PositionActors()
    {
        if (player != null)
            player.position = GetPlayerSpawnWorld();

        if (ghost != null)
            ghost.transform.position = GetGhostSpawnWorld();
    }


    private void GenerateMaze()
    {
        _grid = new int[width, height]; // all walls

        // Start at (1,1) so the outer ring (0 and width/height-1) stays as border walls
        CarveFrom(1, 1);

        // Guarantee a 2x2 open hall at the center
        ForceCenterHall();

        // Punch extra gaps so the maze has multiple paths, not just one solution
        AddGaps(8);
    }

    private void CarveFrom(int x, int y)
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

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            if (InBounds(nx, ny) && _grid[nx, ny] == 0)
            {
                _grid[x + dir.x, y + dir.y] = 1;
                CarveFrom(nx, ny);
            }
        }
    }

    // Forces the 2x2 area at the center of the maze to always be open.
    private void ForceCenterHall()
    {
        int cx = width  / 2; // 3
        int cy = height / 2; // 3

        for (int x = cx; x <= cx + 1; x++)
            for (int y = cy; y <= cy + 1; y++)
                if (InBounds(x, y)) _grid[x, y] = 1;
    }

    // Removes random interior walls that sit between two corridors,
    // creating loops so the maze has more than one solution path.
    private void AddGaps(int count)
    {
        int added = 0, attempts = 0;

        while (added < count && attempts < count * 40)
        {
            attempts++;

            int x = Random.Range(1, width  - 1);
            int y = Random.Range(1, height - 1);

            if (_grid[x, y] == 1) continue; // already open

            bool horizontal = _grid[x - 1, y] == 1 && _grid[x + 1, y] == 1;
            bool vertical   = _grid[x, y - 1] == 1 && _grid[x, y + 1] == 1;

            if (horizontal || vertical)
            {
                _grid[x, y] = 1;
                added++;
            }
        }
    }

    private void BuildMesh()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = CellToWorld(x, y);

                if (_grid[x, y] == 1)
                {
                    Spawn(floorPrefab, pos);
                    Spawn(floorPrefab, new Vector3(pos.x, ceilingHeight, pos.z), Quaternion.Euler(180f, 0f, 0f));
                }
                else
                {
                    Spawn(wallPrefab, pos);
                }
            }
        }
    }

    private void PlaceObjects()
    {
        // Player always spawns at the center hall
        _playerSpawn = new Vector2Int(width / 2, height / 2);

        // Ghost spawns at one of the four inner corners of the maze
        var corners = new List<Vector2Int>
        {
            new(1, 1),
            new(1, height - 2),
            new(width - 2, 1),
            new(width - 2, height - 2)
        };
        _ghostSpawn = corners[Random.Range(0, corners.Count)];

        // Remaining corridors for exit and key pieces (exclude spawn points)
        List<Vector2Int> corridors = GetAllCorridors();
        corridors.Remove(_playerSpawn);
        corridors.Remove(_ghostSpawn);
        Shuffle(corridors);

        int idx = 0;

        // Exit
        _exitCell   = corridors[idx++];
        _exitObject = Spawn(exitPrefab, CellToWorld(_exitCell.x, _exitCell.y));
        if (_exitObject != null)
            _exitObject.SetActive(false);

        // Key pieces
        _keyPieceCells.Clear();
        for (int i = 0; i < keyPieceCount && idx < corridors.Count; i++, idx++)
        {
            _keyPieceCells.Add(corridors[idx]);
            Spawn(keyPiecePrefab, CellToWorld(corridors[idx].x, corridors[idx].y));
        }
    }

    public void RevealExit()
    {
        if (_exitObject != null)
        {
            _exitObject.SetActive(true);
        }
    }

    public Vector3 GetPlayerSpawnWorld()
    {
        Vector3 pos = CellToWorld(_playerSpawn.x, _playerSpawn.y);
        return new Vector3(pos.x, 1.5f, pos.z);
    }

    public Vector3 GetGhostSpawnWorld()
    {
        Vector3 pos = CellToWorld(_ghostSpawn.x, _ghostSpawn.y);
        return new Vector3(pos.x, 1.5f, pos.z);
    }

    public Vector3 GetMazeCenterWorld()
    {
        float cx = (width  - 1) * 0.5f * cellSize;
        float cz = (height - 1) * 0.5f * cellSize;
        return new Vector3(cx, 0f, cz);
    }

    private Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(x * cellSize, 0f, y * cellSize);
    }

    private List<Vector2Int> GetAllCorridors()
    {
        List<Vector2Int> corridors = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] == 1)
                {
                    corridors.Add(new Vector2Int(x, y));
                }
            }
        }

        return corridors;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private GameObject Spawn(GameObject prefab, Vector3 position) =>
        Spawn(prefab, position, Quaternion.identity);

    private GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        GameObject obj = Instantiate(prefab, position, rotation, transform);
        _spawnedObjects.Add(obj);
        return obj;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
