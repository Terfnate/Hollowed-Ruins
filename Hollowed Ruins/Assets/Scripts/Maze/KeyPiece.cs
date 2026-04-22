using UnityEngine;

public class KeyPiece : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float rotateSpeed = 90f;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position + Vector3.up * 1f;
    }

    private void Update()
    {
        float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(_startPos.x, newY, _startPos.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NoiseSystem.Instance?.EmitNoise(transform.position, 999f);
        PieceCollectionSystem.Instance?.CollectPiece();
        Destroy(gameObject);
    }
}
