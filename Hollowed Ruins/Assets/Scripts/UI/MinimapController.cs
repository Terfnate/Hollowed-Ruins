using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float cameraHeight = 40f;
    [SerializeField] private float orthographicSize = 15f;
    [SerializeField] private Rect viewportRect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);

    [Header("Player Marker")]
    [SerializeField] private Transform player;
    [SerializeField] private Color markerColor = Color.cyan;
    [SerializeField] private float markerSize = 3f;

    private Transform _marker;
    private Camera _cam;

    private const int MinimapLayer = 31;

    private void Start()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = orthographicSize;
        _cam.rect = viewportRect;
        _cam.clearFlags = CameraClearFlags.Depth;
        _cam.depth = 1f;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        if (MazeGenerator.Instance != null)
        {
            Vector3 center = MazeGenerator.Instance.GetMazeCenterWorld();
            transform.position = new Vector3(center.x, cameraHeight, center.z);
        }

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        CreateMarker();
        StartCoroutine(HideMarkerFromMainCameraNextFrame());
    }

    private void LateUpdate()
    {
        if (_marker != null && player != null)
            _marker.position = new Vector3(player.position.x, 2.5f, player.position.z);
    }

    private void CreateMarker()
    {
        var markerGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        markerGO.name = "MinimapPlayerMarker";
        markerGO.layer = MinimapLayer;
        markerGO.transform.localScale = new Vector3(markerSize, 0.1f, markerSize);
        Destroy(markerGO.GetComponent<Collider>());

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = markerColor;
        markerGO.GetComponent<Renderer>().material = mat;

        _marker = markerGO.transform;
    }

    // Wait one frame so Camera.main is guaranteed to be the player camera, not this one
    private IEnumerator HideMarkerFromMainCameraNextFrame()
    {
        yield return null;
        if (Camera.main != null)
            Camera.main.cullingMask &= ~(1 << MinimapLayer);
    }
}
