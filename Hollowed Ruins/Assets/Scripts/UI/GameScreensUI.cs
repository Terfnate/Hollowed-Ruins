using UnityEngine;
using UnityEngine.SceneManagement;

// Handles Game Over and Win screen buttons.
public class GameScreensUI : MonoBehaviour
{
    // Called by the Restart button on both Game Over and Win screens
    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Called by the Quit button
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
