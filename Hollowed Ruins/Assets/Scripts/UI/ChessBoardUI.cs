using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Attach to the Chess Duel panel.
// Renders the 4x4 board, handles player clicks, shows highlights.
public class ChessBoardUI : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private GridLayoutGroup boardGrid;     // 4x4 grid layout
    [SerializeField] private GameObject cellPrefab;         // UI cell prefab

    [Header("Cell Colors")]
    [SerializeField] private Color lightCellColor  = new Color(0.9f, 0.85f, 0.75f);
    [SerializeField] private Color darkCellColor   = new Color(0.45f, 0.30f, 0.20f);
    [SerializeField] private Color selectedColor   = new Color(1f, 1f, 0f, 0.6f);
    [SerializeField] private Color validMoveColor  = new Color(0f, 1f, 0.4f, 0.5f);
    [SerializeField] private Color lastMoveColor   = new Color(0.2f, 0.6f, 1f, 0.4f);

    [Header("Piece Sprites")]
    [SerializeField] private Sprite[] whitePieceSprites;    // King,Queen,Rook,Bishop,Knight,Pawn
    [SerializeField] private Sprite[] blackPieceSprites;

    [Header("Objective HUD")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI turnsText;

    private ChessBoardCell[,] _cells = new ChessBoardCell[4, 4];
    private ChessBoard _board;

    void Start()
    {
        ChessDuelManager.Instance.OnDuelStarted         += OnDuelStarted;
        ChessDuelManager.Instance.OnPlayerMoved          += OnMoveExecuted;
        ChessDuelManager.Instance.OnGhostMoved           += OnMoveExecuted;
        ChessDuelManager.Instance.OnTurnsRemainingChanged += OnTurnsChanged;
    }

    // ─── Duel Start ───────────────────────────────────────────────────────────

    void OnDuelStarted(ChessBoard board)
    {
        _board = board;
        BuildGrid();
        RefreshBoard();

        if (objectiveText != null)
            objectiveText.text = ChessDuelManager.Instance
                                    .GetCurrentObjectiveDescription();
    }

    // ─── Grid Building ────────────────────────────────────────────────────────

    void BuildGrid()
    {
        // Clear old cells
        foreach (Transform child in boardGrid.transform)
            Destroy(child.gameObject);

        // Unity GridLayout fills left-to-right, top-to-bottom.
        // Row 3 (top of board) is rendered first.
        for (int row = ChessBoard.SIZE - 1; row >= 0; row--)
        {
            for (int col = 0; col < ChessBoard.SIZE; col++)
            {
                var cellObj = Instantiate(cellPrefab, boardGrid.transform);
                var cell    = cellObj.GetComponent<ChessBoardCell>();

                Vector2Int coords = new(col, row);
                bool isLight = (col + row) % 2 == 0;
                cell.Init(coords, isLight ? lightCellColor : darkCellColor, this);

                _cells[col, row] = cell;
            }
        }
    }

    // ─── Board Refresh ────────────────────────────────────────────────────────

    void RefreshBoard()
    {
        if (_board == null) return;

        // Clear highlights
        ClearHighlights();

        // Place pieces
        for (int col = 0; col < ChessBoard.SIZE; col++)
        {
            for (int row = 0; row < ChessBoard.SIZE; row++)
            {
                ChessPiece piece = _board.GetAt(col, row);
                _cells[col, row].SetPiece(piece, GetSpriteFor(piece));
            }
        }

        // Highlight selected piece and valid moves
        ChessPiece selected = ChessDuelManager.Instance.GetSelectedPiece();
        if (selected != null)
        {
            _cells[selected.Cell.x, selected.Cell.y].SetHighlight(selectedColor);

            foreach (var move in ChessDuelManager.Instance.GetSelectedMoves())
                _cells[move.x, move.y].SetHighlight(validMoveColor);
        }
    }

    void ClearHighlights()
    {
        for (int col = 0; col < ChessBoard.SIZE; col++)
            for (int row = 0; row < ChessBoard.SIZE; row++)
                _cells[col, row].ClearHighlight();
    }

    // ─── Events ───────────────────────────────────────────────────────────────

    public void OnCellClicked(Vector2Int cell)
    {
        ChessDuelManager.Instance.OnCellClicked(cell);
        RefreshBoard();
    }

    void OnMoveExecuted(ChessPiece piece, Vector2Int to)
    {
        RefreshBoard();
    }

    void OnTurnsChanged(int turns)
    {
        if (turnsText != null)
            turnsText.text = $"Turns left: {turns}";
    }

    // ─── Sprite Lookup ────────────────────────────────────────────────────────

    Sprite GetSpriteFor(ChessPiece piece)
    {
        if (piece == null) return null;

        Sprite[] set = piece.Color == PieceColor.White ? whitePieceSprites : blackPieceSprites;
        if (set == null || set.Length == 0) return null;

        int idx = (int)piece.Type;
        return idx < set.Length ? set[idx] : null;
    }
}
