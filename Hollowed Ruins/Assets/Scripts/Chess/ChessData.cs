using UnityEngine;

public enum PieceType  { King, Queen, Rook, Bishop, Knight, Pawn }
public enum PieceColor { White, Black }

public enum ObjectiveType
{
    DontLosePiece,      // player must not lose any piece for N turns
    ProtectPiece,       // a specific piece type must survive for N turns
    CaptureTarget,      // player must capture a specific opponent piece within N turns
    SurviveNTurns       // player's king must avoid check for N turns
}

[System.Serializable]
public class ChessPiece
{
    public PieceType  Type;
    public PieceColor Color;
    public Vector2Int Cell;         // position on 4x4 board (0-3, 0-3)
    public bool       IsCaptured;

    public ChessPiece(PieceType type, PieceColor color, Vector2Int cell)
    {
        Type = type;
        Color = color;
        Cell = cell;
        IsCaptured = false;
    }

    public ChessPiece Clone()
    {
        return new ChessPiece(Type, Color, Cell) { IsCaptured = IsCaptured };
    }
}

[System.Serializable]
public class PiecePlacement
{
    public PieceType  type;
    public PieceColor color;
    public Vector2Int cell;
}

[System.Serializable]
public class ChessObjective
{
    public ObjectiveType type;
    public int turnsAllowed = 3;
    public PieceType targetPieceType;   // used by ProtectPiece and CaptureTarget
    public PieceColor targetPieceColor; // used by CaptureTarget (which side owns it)

    [TextArea]
    public string description;          // shown on HUD e.g. "Protect the Bishop for 3 turns"
}
