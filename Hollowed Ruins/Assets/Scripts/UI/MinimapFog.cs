using UnityEngine;

public class MinimapFog : MonoBehaviour
{
    [SerializeField] private float fogHeight = 25f;

    private const int FogLayer = 30;

    private GameObject[,] _fogQuads;
    private bool[,] _visited;
    private Vector2Int _lastCell = new(-1, -1);
    private float _cellSize;
    private int _width, _height;
    private Transform _player;

    public void OnMazeReady(int width, int height, float cellSize, Vector2Int playerSpawn)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _fogQuads = new GameObject[width, height];
        _visited = new bool[width, height];

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = new Color(0.05f, 0.05f, 0.05f, 1f);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = $"FogQuad_{x}_{y}";
                quad.layer = FogLayer;
                quad.transform.SetParent(transform);
                quad.transform.position = new Vector3(x * cellSize, fogHeight, y * cellSize);
                quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                quad.transform.localScale = new Vector3(cellSize, cellSize, 1f);
                quad.GetComponent<Renderer>().sharedMaterial = mat;
                Destroy(quad.GetComponent<Collider>());
                _fogQuads[x, y] = quad;
            }
        }

        RevealArea(playerSpawn.x, playerSpawn.y);
        _lastCell = playerSpawn;
    }

    private void Update()
    {
        if (_fogQuads == null) return;

        if (_player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go == null) return;
            _player = go.transform;
        }

        int cx = Mathf.Clamp(Mathf.FloorToInt(_player.position.x / _cellSize), 0, _width - 1);
        int cy = Mathf.Clamp(Mathf.FloorToInt(_player.position.z / _cellSize), 0, _height - 1);

        if (cx == _lastCell.x && cy == _lastCell.y) return;
        _lastCell = new Vector2Int(cx, cy);

        RevealArea(cx, cy);
    }

    private void RevealArea(int cx, int cy)
    {
        Reveal(cx, cy);
        Reveal(cx + 1, cy);
        Reveal(cx - 1, cy);
        Reveal(cx, cy + 1);
        Reveal(cx, cy - 1);
    }

    private void Reveal(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return;
        if (_visited[x, y]) return;
        _visited[x, y] = true;
        if (_fogQuads[x, y] != null)
            _fogQuads[x, y].SetActive(false);
    }
}
