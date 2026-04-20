using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapController : MonoBehaviour
{
    [SerializeField] private float cameraHeight = 40f;
    [SerializeField] private float orthographicSize = 15f;

    private void Start()
    {
        var cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        if (MazeGenerator.Instance != null)
        {
            Vector3 center = MazeGenerator.Instance.GetMazeCenterWorld();
            transform.position = new Vector3(center.x, cameraHeight, center.z);
        }
    }
}
