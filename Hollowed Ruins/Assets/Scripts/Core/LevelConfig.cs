using UnityEngine;

[CreateAssetMenu(menuName = "Hollowed Ruins/Level Config", fileName = "LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("Maze")]
    public int mazeWidth    = 7;
    public int mazeHeight   = 7;

    [Header("Keys")]
    public int keyPieceCount = 5;

    [Header("Ghost")]
    public float ghostPatrolSpeed = 4f;
    public float ghostChaseSpeed  = 8f;
}
