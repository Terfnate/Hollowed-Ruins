using UnityEngine;
using System.Collections.Generic;

// Create via: Assets > Create > Hollowed Ruins > Chess Scenario
[CreateAssetMenu(menuName = "Hollowed Ruins/Chess Scenario", fileName = "NewChessScenario")]
public class ChessScenario : ScriptableObject
{
    [Header("Board Setup")]
    public List<PiecePlacement> pieces = new();

    [Header("Objective")]
    public ChessObjective objective;
}
