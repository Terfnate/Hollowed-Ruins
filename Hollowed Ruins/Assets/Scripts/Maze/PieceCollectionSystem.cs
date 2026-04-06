using UnityEngine;
using UnityEngine.Events;

public class PieceCollectionSystem : MonoBehaviour
{
    public static PieceCollectionSystem Instance { get; private set; }

    [SerializeField] private int totalPieces = 5;

    public int CollectedCount { get; private set; }
    public int TotalPieces => totalPieces;

    // Standard C# events — no UnityEvent<T,T> to avoid Unity serialization issues
    public event System.Action<int, int> OnPieceCollected;  // (collected, total)
    public event System.Action OnAllPiecesCollected;
    public UnityEvent OnAllPiecesCollectedUnity;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void CollectPiece()
    {
        CollectedCount++;
        OnPieceCollected?.Invoke(CollectedCount, totalPieces);

        if (CollectedCount >= totalPieces)
        {
            OnAllPiecesCollected?.Invoke();
            MazeGenerator.Instance?.RevealExit();
        }
    }

    public bool AllCollected() => CollectedCount >= totalPieces;
}
