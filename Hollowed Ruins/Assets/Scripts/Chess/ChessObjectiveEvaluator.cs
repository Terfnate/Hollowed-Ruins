using UnityEngine;

public enum ObjectiveResult { Ongoing, PlayerWin, PlayerLose }

public class ChessObjectiveEvaluator
{
    private ChessObjective _objective;
    private int _turnsRemaining;

    public int TurnsRemaining => _turnsRemaining;

    public void Load(ChessObjective objective)
    {
        _objective      = objective;
        _turnsRemaining = objective.turnsAllowed;
    }

    // Called after the PLAYER moves — only checks for immediate player win, no turn decrement.
    public ObjectiveResult EvaluateAfterPlayerMove(ChessBoard board, ChessPiece captured)
    {
        if (_objective.type == ObjectiveType.CaptureTarget &&
            captured != null &&
            captured.Color == _objective.targetPieceColor &&
            captured.Type  == _objective.targetPieceType)
            return ObjectiveResult.PlayerWin;

        return ObjectiveResult.Ongoing;
    }

    // Called after the GHOST moves — checks loss conditions, then decrements one player turn.
    public ObjectiveResult EvaluateAfterGhostMove(ChessBoard board, ChessPiece captured)
    {
        // Ghost captured a white piece — check loss conditions
        if (captured != null && captured.Color == PieceColor.White)
        {
            if (_objective.type == ObjectiveType.DontLosePiece)
                return ObjectiveResult.PlayerLose;

            if (_objective.type == ObjectiveType.ProtectPiece &&
                captured.Type == _objective.targetPieceType)
                return ObjectiveResult.PlayerLose;
        }

        // One full player turn has passed
        _turnsRemaining--;

        if (_turnsRemaining <= 0)
        {
            return _objective.type == ObjectiveType.CaptureTarget
                ? ObjectiveResult.PlayerLose   // ran out of time to capture
                : ObjectiveResult.PlayerWin;   // survived all turns
        }

        return ObjectiveResult.Ongoing;
    }
}
