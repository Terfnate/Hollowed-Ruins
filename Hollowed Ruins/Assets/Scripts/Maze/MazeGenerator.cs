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
    }

    void BakeNavMesh()
    {
        var surface = GetComponent<NavMeshSurface>();
        // Collect only children of this GameObject (the spawned floor/wall tiles)
        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry    = NavMeshCollectGeometry.RenderMeshes;
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
        _grid = new int[width, height];
        CarveFrom(0, 0);
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
        List<Vector2Int> corridors = GetAllCorridors();
        Shuffle(corridors);

        int idx = 0;

        _playerSpawn = corridors[idx++];
        _ghostSpawn = corridors[corridors.Count - 1];

        _exitCell = corridors[corridors.Count - 2];
        _exitObject = Spawn(exitPrefab, CellToWorld(_exitCell.x, _exitCell.y));
        if (_exitObject != null)
        {
            _exitObject.SetActive(false);
        }

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
        return new Vector3(pos.x, 0.5f, pos.z);
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

    private GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
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
