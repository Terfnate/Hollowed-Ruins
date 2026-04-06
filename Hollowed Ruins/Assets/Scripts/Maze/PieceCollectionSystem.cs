using UnityEngine;
using UnityEngine.Events;

public class PieceCollectionSystem : MonoBehaviour
{
    public static PieceCollectionSystem Instance { get; private set; }

    [SerializeField] private int totalPieces = 5;

    public int CollectedCount { get; private set; }

    public UnityEvent<int, int> OnPieceCollected;   // collected, total
    public UnityEvent OnAllPiecesCollected;

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
