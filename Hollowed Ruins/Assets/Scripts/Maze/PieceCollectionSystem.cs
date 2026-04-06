using UnityEngine;
using UnityEngine.Events;

public class PieceCollectionSystem : MonoBehaviour
{
    public static PieceCollectionSystem Instance { get; private set; }

    [SerializeField] private int totalPieces = 5;

    public int CollectedCount { get; private set; }
    public int TotalPieces => totalPieces;

    public event System.Action<int, int> OnPieceCollected;
    public event System.Action OnAllPiecesCollected;
    public UnityEvent OnAllPiecesCollectedUnity = new();

    private void Awake()
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

        if (CollectedCount < totalPieces)
        {
            return;
        }

        OnAllPiecesCollected?.Invoke();
        OnAllPiecesCollectedUnity?.Invoke();
        MazeGenerator.Instance?.RevealExit();
    }

    public bool AllCollected()
    {
        return CollectedCount >= totalPieces;
    }
}
