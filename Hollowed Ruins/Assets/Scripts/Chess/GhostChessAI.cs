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

        ChessPiece bestPiece  = null;
        Vector2Int bestTarget = Vector2Int.zero;
        int        bestScore  = int.MinValue;

        foreach (var piece in blackPieces)
        {
            foreach (var target in _board.GetLegalMoves(piece))
            {
                var clone = _board.Clone();
                clone.ExecuteMove(clone.GetAt(piece.Cell), target);

                int score = Minimax(clone, depth: 1, maximizing: false);
                if (score > bestScore)
                {
                    bestScore  = score;
                    bestPiece  = piece;
                    bestTarget = target;
                }
            }
        }

        return (bestPiece, bestTarget);
    }

    // Depth-2 minimax: ghost maximises, player minimises.
    int Minimax(ChessBoard board, int depth, bool maximizing)
    {
        if (depth == 0) return Evaluate(board);

        PieceColor side   = maximizing ? PieceColor.Black : PieceColor.White;
        var        pieces = board.GetPiecesOf(side);

        if (pieces.Count == 0)
            return maximizing ? int.MinValue : int.MaxValue;

        int  best     = maximizing ? int.MinValue : int.MaxValue;
        bool hasMoves = false;

        foreach (var piece in pieces)
        {
            foreach (var target in board.GetLegalMoves(piece))
            {
                hasMoves = true;
                var clone = board.Clone();
                clone.ExecuteMove(clone.GetAt(piece.Cell), target);

                int score = Minimax(clone, depth - 1, !maximizing);
                best = maximizing ? Mathf.Max(best, score) : Mathf.Min(best, score);
            }
        }

        return hasMoves ? best : Evaluate(board);
    }

    // Positive = good for black (ghost).
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
