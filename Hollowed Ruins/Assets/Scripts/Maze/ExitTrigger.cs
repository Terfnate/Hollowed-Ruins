using UnityEngine;

// Attach to the exit prefab.
// Only works after all pieces are collected.
public class ExitTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!PieceCollectionSystem.Instance.AllCollected()) return;

        GameStateManager.Instance.SetState(GameState.Win);
    }
}
