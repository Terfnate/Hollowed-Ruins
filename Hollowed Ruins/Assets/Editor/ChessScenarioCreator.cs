using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class ChessScenarioCreator
{
    private const string SavePath = "Assets/Scripts/Chess/Scenarios";

    [MenuItem("Hollowed Ruins/Create Chess Scenarios")]
    static void CreateAll()
    {
        Directory.CreateDirectory(Application.dataPath + "/Scripts/Chess/Scenarios");
        AssetDatabase.Refresh();

        Create_HoldTheLine();
        Create_GuardTheBishop();
        Create_HuntTheKnight();
        Create_LastKingStanding();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All 4 Chess Scenarios created in " + SavePath);
    }

    // ── Scenario 1: Hold the Line ─────────────────────────────────────────────
    // White Rook + Pawn vs Black Bishop + Knight. Survive 3 turns.
    static void Create_HoldTheLine()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Rook,   PieceColor.White, 0, 0),
            P(PieceType.Pawn,   PieceColor.White, 2, 0),
            P(PieceType.Bishop, PieceColor.Black, 1, 3),
            P(PieceType.Knight, PieceColor.Black, 3, 2),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.DontLosePiece,
            turnsAllowed = 3,
            description  = "Don't lose any pieces for 3 turns"
        };
        Save(s, "Scenario_HoldTheLine");
    }

    // ── Scenario 2: Guard the Bishop ──────────────────────────────────────────
    // White Bishop + Pawn vs Black Queen + Pawn. Keep Bishop alive for 3 turns.
    static void Create_GuardTheBishop()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Bishop, PieceColor.White, 1, 1),
            P(PieceType.Pawn,   PieceColor.White, 0, 0),
            P(PieceType.Queen,  PieceColor.Black, 3, 3),
            P(PieceType.Pawn,   PieceColor.Black, 2, 2),
        };
        s.objective = new ChessObjective
        {
            type            = ObjectiveType.ProtectPiece,
            turnsAllowed    = 3,
            targetPieceType = PieceType.Bishop,
            description     = "Keep your Bishop alive for 3 turns"
        };
        Save(s, "Scenario_GuardTheBishop");
    }

    // ── Scenario 3: Hunt the Knight ───────────────────────────────────────────
    // White Queen + Pawn vs Black Knight + Pawn. Capture the Knight in 3 turns.
    static void Create_HuntTheKnight()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Queen,  PieceColor.White, 0, 0),
            P(PieceType.Pawn,   PieceColor.White, 1, 1),
            P(PieceType.Knight, PieceColor.Black, 2, 3),
            P(PieceType.Pawn,   PieceColor.Black, 3, 2),
        };
        s.objective = new ChessObjective
        {
            type             = ObjectiveType.CaptureTarget,
            turnsAllowed     = 3,
            targetPieceType  = PieceType.Knight,
            targetPieceColor = PieceColor.Black,
            description      = "Capture the enemy Knight within 3 turns"
        };
        Save(s, "Scenario_HuntTheKnight");
    }

    // ── Scenario 4: Last King Standing ────────────────────────────────────────
    // White King + Rook vs Black Queen + Bishop + Pawn. Survive 4 turns.
    static void Create_LastKingStanding()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.King,   PieceColor.White, 1, 0),
            P(PieceType.Rook,   PieceColor.White, 0, 1),
            P(PieceType.Queen,  PieceColor.Black, 3, 3),
            P(PieceType.Bishop, PieceColor.Black, 2, 3),
            P(PieceType.Pawn,   PieceColor.Black, 1, 3),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.SurviveNTurns,
            turnsAllowed = 4,
            description  = "Survive 4 turns — don't let your King fall"
        };
        Save(s, "Scenario_LastKingStanding");
    }

    static PiecePlacement P(PieceType type, PieceColor color, int x, int y)
        => new PiecePlacement { type = type, color = color, cell = new Vector2Int(x, y) };

    static void Save(ChessScenario scenario, string name)
    {
        string path = SavePath + "/" + name + ".asset";
        AssetDatabase.CreateAsset(scenario, path);
    }
}
