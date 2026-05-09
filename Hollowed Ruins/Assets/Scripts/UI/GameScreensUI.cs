using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScreensUI : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        GameStateManager.Instance?.ResetForRestart();
        HealthSystem.Instance?.ResetHearts();
        PieceCollectionSystem.Instance?.ResetPieces();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        GameStateManager.Instance?.ResetForRestart();
        HealthSystem.Instance?.ResetHearts();
        PieceCollectionSystem.Instance?.ResetPieces();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
