using UnityEngine;
using System.Collections.Generic;

// Pure logic — no MonoBehaviour, no Unity lifecycle.
// ChessDuelManager owns and drives this.
public class ChessBoard
{
    public const int SIZE = 4;

    // Indexed [col, row] — (0,0) is bottom-left
    private ChessPiece[,] _grid = new ChessPiece[SIZE, SIZE];
    private List<ChessPiece> _pieces = new();

    // ─── Setup ────────────────────────────────────────────────────────────────

    public void LoadScenario(ChessScenario scenario)
    {
        _grid = new ChessPiece[SIZE, SIZE];
        _pieces.Clear();

        foreach (var placement in scenario.pieces)
        {
            var piece = new ChessPiece(placement.type, placement.color, placement.cell);
            _grid[placement.cell.x, placement.cell.y] = piece;
            _pieces.Add(piece);
        }
    }

    // ─── Accessors ────────────────────────────────────────────────────────────

    public ChessPiece GetAt(Vector2Int cell) => InBounds(cell) ? _grid[cell.x, cell.y] : null;
    public ChessPiece GetAt(int x, int y)    => InBounds(x, y) ? _grid[x, y] : null;
    public List<ChessPiece> GetAllPieces()   => _pieces;

    public List<ChessPiece> GetPiecesOf(PieceColor color)
    {
        var list = new List<ChessPiece>();
        foreach (var p in _pieces)
            if (!p.IsCaptured && p.Color == color) list.Add(p);
        return list;
    }

    // ─── Move Execution ───────────────────────────────────────────────────────

    // Returns the captured piece (or null).
    public ChessPiece ExecuteMove(ChessPiece piece, Vector2Int to)
    {
        ChessPiece captured = GetAt(to);

        if (captured != null)
        {
            captured.IsCaptured = true;
            _grid[to.x, to.y] = null;
        }

        _grid[piece.Cell.x, piece.Cell.y] = null;
        piece.Cell = to;
        _grid[to.x, to.y] = piece;

        return captured;
    }

    // ─── Move Generation ──────────────────────────────────────────────────────

    public List<Vector2Int> GetLegalMoves(ChessPiece piece)
    {
        var moves = new List<Vector2Int>();

        switch (piece.Type)
        {
            case PieceType.Pawn:   AddPawnMoves(piece, moves);   break;
            case PieceType.Rook:   AddSlideMoves(piece, moves, rookDirs);    break;
            case PieceType.Bishop: AddSlideMoves(piece, moves, bishopDirs);  break;
            case PieceType.Queen:  AddSlideMoves(piece, moves, queenDirs);   break;
            case PieceType.King:   AddKingMoves(piece, moves);   break;
            case PieceType.Knight: AddKnightMoves(piece, moves); break;
        }

        return moves;
    }

    static readonly Vector2Int[] rookDirs   = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    static readonly Vector2Int[] bishopDirs = { new(1,1), new(1,-1), new(-1,1), new(-1,-1) };
    static readonly Vector2Int[] queenDirs  = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                                                new(1,1), new(1,-1), new(-1,1), new(-1,-1) };

    void AddSlideMoves(ChessPiece piece, List<Vector2Int> moves, Vector2Int[] dirs)
    {
        foreach (var dir in dirs)
        {
            Vector2Int cur = piece.Cell + dir;
            while (InBounds(cur))
            {
                ChessPiece blocker = GetAt(cur);
                if (blocker == null)
                {
                    moves.Add(cur);
                }
                else
                {
                    if (blocker.Color != piece.Color) moves.Add(cur); // capture
                    break;
                }
                cur += dir;
            }
        }
    }

    void AddKingMoves(ChessPiece piece, List<Vector2Int> moves)
    {
        foreach (var dir in queenDirs)
        {
            Vector2Int target = piece.Cell + dir;
            if (InBounds(target))
            {
                ChessPiece occupant = GetAt(target);
                if (occupant == null || occupant.Color != piece.Color)
                    moves.Add(target);
            }
        }
    }

    void AddKnightMoves(ChessPiece piece, List<Vector2Int> moves)
    {
        Vector2Int[] jumps =
        {
            new(2,1), new(2,-1), new(-2,1), new(-2,-1),
            new(1,2), new(1,-2), new(-1,2), new(-1,-2)
        };

        foreach (var jump in jumps)
        {
            Vector2Int target = piece.Cell + jump;
            if (InBounds(target))
            {
                ChessPiece occupant = GetAt(target);
                if (occupant == null || occupant.Color != piece.Color)
                    moves.Add(target);
            }
        }
    }

    void AddPawnMoves(ChessPiece piece, List<Vector2Int> moves)
    {
        // White moves up (+y), Black moves down (-y)
        int dir = piece.Color == PieceColor.White ? 1 : -1;
        Vector2Int forward = new(piece.Cell.x, piece.Cell.y + dir);

        if (InBounds(forward) && GetAt(forward) == null)
            moves.Add(forward);

        // Diagonal captures
        foreach (int dx in new[] { -1, 1 })
        {
            Vector2Int capture = new(piece.Cell.x + dx, piece.Cell.y + dir);
            if (InBounds(capture))
            {
                ChessPiece target = GetAt(capture);
                if (target != null && target.Color != piece.Color)
                    moves.Add(capture);
            }
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    bool InBounds(Vector2Int c) => c.x >= 0 && c.x < SIZE && c.y >= 0 && c.y < SIZE;
    bool InBounds(int x, int y) => x >= 0 && x < SIZE && y >= 0 && y < SIZE;
}
