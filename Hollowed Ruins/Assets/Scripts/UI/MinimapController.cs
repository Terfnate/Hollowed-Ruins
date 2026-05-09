using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private float cameraHeight = 40f;
    [SerializeField] private float orthographicSize = 15f;
    [SerializeField] private Rect viewportRect = new Rect(0.75f, 0.75f, 0.25f, 0.25f);

    private Camera _cam;

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
    }
}
