using UnityEngine;

public enum ObjectiveResult { Ongoing, PlayerWin, PlayerLose }

public class ChessObjectiveEvaluator
{
    private ChessObjective _objective;
    private int _turnsRemaining;
    private bool _objectiveFailed;

    public int TurnsRemaining => _turnsRemaining;

    public void Load(ChessObjective objective)
    {
        _objective      = objective;
        _turnsRemaining = objective.turnsAllowed;
        _objectiveFailed = false;
    }

    // Called after EACH full round (player turn + ghost turn).
    public ObjectiveResult Evaluate(ChessBoard board, ChessPiece lastCaptured)
    {
        if (_objectiveFailed) return ObjectiveResult.PlayerLose;

        switch (_objective.type)
        {
            case ObjectiveType.DontLosePiece:
                if (lastCaptured != null && lastCaptured.Color == PieceColor.White)
                    return ObjectiveResult.PlayerLose;
                break;

            case ObjectiveType.ProtectPiece:
                if (lastCaptured != null &&
                    lastCaptured.Color == PieceColor.White &&
                    lastCaptured.Type  == _objective.targetPieceType)
                    return ObjectiveResult.PlayerLose;
                break;

            case ObjectiveType.CaptureTarget:
                // Check if the target piece has been captured
                bool targetCaptured = false;
                foreach (var piece in board.GetAllPieces())
                {
                    if (piece.Color == _objective.targetPieceColor &&
                        piece.Type  == _objective.targetPieceType  &&
                        piece.IsCaptured)
                    {
                        targetCaptured = true;
                        break;
                    }
                }
                if (targetCaptured) return ObjectiveResult.PlayerWin;
                break;

            case ObjectiveType.SurviveNTurns:
                // Fail immediately if player's king is in check
                // (simplified: just survive all turns)
                break;
        }

        _turnsRemaining--;

        if (_turnsRemaining <= 0)
        {
            // Survived all turns — win for survive-type objectives
            if (_objective.type == ObjectiveType.DontLosePiece  ||
                _objective.type == ObjectiveType.ProtectPiece   ||
                _objective.type == ObjectiveType.SurviveNTurns)
                return ObjectiveResult.PlayerWin;

            // Failed to capture in time
            if (_objective.type == ObjectiveType.CaptureTarget)
                return ObjectiveResult.PlayerLose;
        }

        return ObjectiveResult.Ongoing;
    }
}
