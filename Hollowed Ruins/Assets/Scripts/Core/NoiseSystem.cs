using UnityEngine;
using UnityEngine.Events;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance { get; private set; }

    // Ghost subscribes to this to get alerted
    public UnityEvent<Vector3, float> OnNoiseEmitted;  // position, radius

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void EmitNoise(Vector3 position, float radius)
    {
        OnNoiseEmitted?.Invoke(position, radius);
    }
}
