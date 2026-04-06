using UnityEngine;
using UnityEngine.UI;

// Attach to the cell prefab used by ChessBoardUI.
// Each cell = one square on the 4x4 board.
public class ChessBoardCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Image pieceImage;

    private Vector2Int   _coords;
    private Color        _baseColor;
    private ChessBoardUI _boardUI;

    public void Init(Vector2Int coords, Color baseColor, ChessBoardUI boardUI)
    {
        _coords   = coords;
        _baseColor = baseColor;
        _boardUI  = boardUI;

        if (backgroundImage != null) backgroundImage.color = baseColor;
        if (highlightImage  != null) highlightImage.color  = Color.clear;
        if (pieceImage      != null) pieceImage.sprite      = null;
    }

    public void SetPiece(ChessPiece piece, Sprite sprite)
    {
        if (pieceImage == null) return;

        pieceImage.sprite  = sprite;
        pieceImage.enabled = sprite != null;
        pieceImage.color   = piece?.Color == PieceColor.White
                             ? Color.white
                             : new Color(0.15f, 0.15f, 0.15f);
    }

    public void SetHighlight(Color color)
    {
        if (highlightImage != null)
            highlightImage.color = color;
    }

    public void ClearHighlight()
    {
        if (highlightImage != null)
            highlightImage.color = Color.clear;
    }

    public void OnClick()
    {
        _boardUI?.OnCellClicked(_coords);
    }
}
