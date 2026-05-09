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

        // Original 4
        Create_HoldTheLine();
        Create_GuardTheBishop();
        Create_HuntTheKnight();
        Create_LastKingStanding();

        // New 10
        Create_TwinTowers();
        Create_QueenHunt();
        Create_TheFortress();
        Create_PawnWave();
        Create_ProtectTheKing();
        Create_RookCapture();
        Create_DoubleBishop();
        Create_KnightsChallenge();
        Create_TheSiege();
        Create_BishopsStand();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All 14 Chess Scenarios created in " + SavePath);
    }

    // ── Scenario 1: Hold the Line ─────────────────────────────────────────────
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

    // ── Scenario 5: Twin Towers ───────────────────────────────────────────────
    // White: two Rooks. Black: Queen + Bishop. Survive 4 turns.
    static void Create_TwinTowers()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Rook,   PieceColor.White, 0, 0),
            P(PieceType.Rook,   PieceColor.White, 3, 0),
            P(PieceType.Queen,  PieceColor.Black, 1, 3),
            P(PieceType.Bishop, PieceColor.Black, 2, 3),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.DontLosePiece,
            turnsAllowed = 4,
            description  = "Protect both Rooks for 4 turns"
        };
        Save(s, "Scenario_TwinTowers");
    }

    // ── Scenario 6: Queen Hunt ────────────────────────────────────────────────
    // White: Rook + Knight. Black: Queen + Pawn. Capture the Queen in 4 turns.
    static void Create_QueenHunt()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Rook,   PieceColor.White, 0, 0),
            P(PieceType.Knight, PieceColor.White, 2, 0),
            P(PieceType.Queen,  PieceColor.Black, 3, 3),
            P(PieceType.Pawn,   PieceColor.Black, 3, 2),
        };
        s.objective = new ChessObjective
        {
            type             = ObjectiveType.CaptureTarget,
            turnsAllowed     = 4,
            targetPieceType  = PieceType.Queen,
            targetPieceColor = PieceColor.Black,
            description      = "Capture the enemy Queen within 4 turns"
        };
        Save(s, "Scenario_QueenHunt");
    }

    // ── Scenario 7: The Fortress ──────────────────────────────────────────────
    // White: King + Rook + Bishop. Black: Queen + Rook. Survive 3 turns.
    static void Create_TheFortress()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.King,   PieceColor.White, 0, 0),
            P(PieceType.Rook,   PieceColor.White, 1, 0),
            P(PieceType.Bishop, PieceColor.White, 0, 1),
            P(PieceType.Queen,  PieceColor.Black, 3, 3),
            P(PieceType.Rook,   PieceColor.Black, 3, 2),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.SurviveNTurns,
            turnsAllowed = 3,
            description  = "Hold the fortress for 3 turns"
        };
        Save(s, "Scenario_TheFortress");
    }

    // ── Scenario 8: Pawn Wave ─────────────────────────────────────────────────
    // White: Knight + Bishop. Black: 4 Pawns charging down. Survive 3 turns.
    static void Create_PawnWave()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Knight, PieceColor.White, 1, 1),
            P(PieceType.Bishop, PieceColor.White, 2, 0),
            P(PieceType.Pawn,   PieceColor.Black, 0, 3),
            P(PieceType.Pawn,   PieceColor.Black, 1, 3),
            P(PieceType.Pawn,   PieceColor.Black, 2, 3),
            P(PieceType.Pawn,   PieceColor.Black, 3, 3),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.DontLosePiece,
            turnsAllowed = 3,
            description  = "Survive the pawn wave for 3 turns"
        };
        Save(s, "Scenario_PawnWave");
    }

    // ── Scenario 9: Protect the King ─────────────────────────────────────────
    // White: King + Pawn. Black: two Rooks + Bishop. Keep King alive 4 turns.
    static void Create_ProtectTheKing()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.King,   PieceColor.White, 1, 0),
            P(PieceType.Pawn,   PieceColor.White, 0, 0),
            P(PieceType.Rook,   PieceColor.Black, 0, 3),
            P(PieceType.Rook,   PieceColor.Black, 3, 3),
            P(PieceType.Bishop, PieceColor.Black, 1, 3),
        };
        s.objective = new ChessObjective
        {
            type            = ObjectiveType.ProtectPiece,
            turnsAllowed    = 4,
            targetPieceType = PieceType.King,
            description     = "Keep your King alive for 4 turns"
        };
        Save(s, "Scenario_ProtectTheKing");
    }

    // ── Scenario 10: Rook Capture ─────────────────────────────────────────────
    // White: Queen + Pawn. Black: Rook + Knight. Capture the Rook in 3 turns.
    static void Create_RookCapture()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Queen,  PieceColor.White, 0, 0),
            P(PieceType.Pawn,   PieceColor.White, 1, 0),
            P(PieceType.Rook,   PieceColor.Black, 2, 3),
            P(PieceType.Knight, PieceColor.Black, 3, 3),
        };
        s.objective = new ChessObjective
        {
            type             = ObjectiveType.CaptureTarget,
            turnsAllowed     = 3,
            targetPieceType  = PieceType.Rook,
            targetPieceColor = PieceColor.Black,
            description      = "Capture the enemy Rook within 3 turns"
        };
        Save(s, "Scenario_RookCapture");
    }

    // ── Scenario 11: Double Bishop ────────────────────────────────────────────
    // White: 2 Bishops + Pawn. Black: Queen + Knight. Survive 3 turns.
    static void Create_DoubleBishop()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Bishop, PieceColor.White, 0, 0),
            P(PieceType.Bishop, PieceColor.White, 3, 0),
            P(PieceType.Pawn,   PieceColor.White, 1, 0),
            P(PieceType.Queen,  PieceColor.Black, 2, 3),
            P(PieceType.Knight, PieceColor.Black, 1, 3),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.DontLosePiece,
            turnsAllowed = 3,
            description  = "Keep both Bishops alive for 3 turns"
        };
        Save(s, "Scenario_DoubleBishop");
    }

    // ── Scenario 12: Knight's Challenge ──────────────────────────────────────
    // White: 2 Knights. Black: Rook + Bishop + Pawn. Survive 4 turns.
    static void Create_KnightsChallenge()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Knight, PieceColor.White, 0, 0),
            P(PieceType.Knight, PieceColor.White, 3, 0),
            P(PieceType.Rook,   PieceColor.Black, 1, 3),
            P(PieceType.Bishop, PieceColor.Black, 2, 2),
            P(PieceType.Pawn,   PieceColor.Black, 3, 3),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.DontLosePiece,
            turnsAllowed = 4,
            description  = "Keep your Knights alive for 4 turns"
        };
        Save(s, "Scenario_KnightsChallenge");
    }

    // ── Scenario 13: The Siege ────────────────────────────────────────────────
    // White: King + Rook. Black: Queen + Rook + Knight. Survive 4 turns.
    static void Create_TheSiege()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.King,   PieceColor.White, 2, 0),
            P(PieceType.Rook,   PieceColor.White, 0, 0),
            P(PieceType.Queen,  PieceColor.Black, 3, 3),
            P(PieceType.Rook,   PieceColor.Black, 0, 3),
            P(PieceType.Knight, PieceColor.Black, 2, 3),
        };
        s.objective = new ChessObjective
        {
            type         = ObjectiveType.SurviveNTurns,
            turnsAllowed = 4,
            description  = "Withstand the siege for 4 turns"
        };
        Save(s, "Scenario_TheSiege");
    }

    // ── Scenario 14: Bishop's Stand ───────────────────────────────────────────
    // White: Bishop + Rook. Black: Queen + Knight + Pawn. Keep Bishop alive 3 turns.
    static void Create_BishopsStand()
    {
        var s = ScriptableObject.CreateInstance<ChessScenario>();
        s.pieces = new List<PiecePlacement>
        {
            P(PieceType.Bishop, PieceColor.White, 1, 1),
            P(PieceType.Rook,   PieceColor.White, 0, 0),
            P(PieceType.Queen,  PieceColor.Black, 3, 3),
            P(PieceType.Knight, PieceColor.Black, 2, 3),
            P(PieceType.Pawn,   PieceColor.Black, 1, 3),
        };
        s.objective = new ChessObjective
        {
            type            = ObjectiveType.ProtectPiece,
            turnsAllowed    = 3,
            targetPieceType = PieceType.Bishop,
            description     = "Keep your Bishop alive for 3 turns"
        };
        Save(s, "Scenario_BishopsStand");
    }

    static PiecePlacement P(PieceType type, PieceColor color, int x, int y)
        => new PiecePlacement { type = type, color = color, cell = new Vector2Int(x, y) };

    static void Save(ChessScenario scenario, string name)
    {
        string path = SavePath + "/" + name + ".asset";
        AssetDatabase.CreateAsset(scenario, path);
    }
}
