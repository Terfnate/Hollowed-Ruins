using UnityEngine;

// Coordinates which UI panel is visible based on GameState.
// Assign all panels in the Inspector.
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject chessDuelPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    void Start()
    {
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        OnStateChanged(GameStateManager.Instance.CurrentState);
    }

    void OnStateChanged(GameState state)
    {
        hudPanel?.SetActive(state == GameState.Exploring);
        chessDuelPanel?.SetActive(state == GameState.ChessDuel);
        gameOverPanel?.SetActive(state == GameState.GameOver);
        winPanel?.SetActive(state == GameState.Win);
    }
}
