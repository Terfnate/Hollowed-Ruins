using UnityEngine;
using System.Collections.Generic;

// Ghost's move selection logic.
// Priority: capture a player piece > random legal move.
public class GhostChessAI
{
    private ChessBoard _board;

    public GhostChessAI(ChessBoard board)
    {
        _board = board;
    }

    // Returns (piece, targetCell) for the ghost's chosen move.
    public (ChessPiece piece, Vector2Int to) PickMove()
    {
        var ghosts = _board.GetPiecesOf(PieceColor.Black);

        // Collect all moves that capture a white piece
        var captureMoves = new List<(ChessPiece, Vector2Int)>();
        var allMoves     = new List<(ChessPiece, Vector2Int)>();

        foreach (var piece in ghosts)
        {
            var legal = _board.GetLegalMoves(piece);
            foreach (var target in legal)
            {
                allMoves.Add((piece, target));
                if (_board.GetAt(target) != null)
                    captureMoves.Add((piece, target));
            }
        }

        // Prefer captures
        if (captureMoves.Count > 0)
            return captureMoves[Random.Range(0, captureMoves.Count)];

        if (allMoves.Count > 0)
            return allMoves[Random.Range(0, allMoves.Count)];

        // No moves available (shouldn't happen in valid scenarios)
        return (null, Vector2Int.zero);
    }
}
