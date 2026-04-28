using UnityEngine;
using System.Collections.Generic;

public class GhostChessAI
{
    private ChessBoard _board;

    // King, Queen, Rook, Bishop, Knight, Pawn
    static readonly int[] PieceValues = { 100, 9, 5, 3, 3, 1 };

    public GhostChessAI(ChessBoard board) => _board = board;

    public (ChessPiece piece, Vector2Int to) PickMove()
    {
        var blackPieces = _board.GetPiecesOf(PieceColor.Black);

        ChessPiece bestPiece = null;
        Vector2Int bestTarget = Vector2Int.zero;
        int bestScore = int.MinValue;

        foreach (var piece in blackPieces)
        {
            foreach (var target in _board.GetLegalMoves(piece))
            {
                var clone = _board.Clone();
                var clonedPiece = clone.GetAt(piece.Cell);
                clone.ExecuteMove(clonedPiece, target);

                int score = Evaluate(clone);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPiece = piece;
                    bestTarget = target;
                }
            }
        }

        return (bestPiece, bestTarget);
    }

    // Positive = good for black (ghost), negative = good for white (player)
    int Evaluate(ChessBoard board)
    {
        int score = 0;
        foreach (var p in board.GetAllPieces())
        {
            if (p.IsCaptured) continue;
            int val = PieceValues[(int)p.Type];
            score += p.Color == PieceColor.Black ? val : -val;
        }
        return score;
    }
}
