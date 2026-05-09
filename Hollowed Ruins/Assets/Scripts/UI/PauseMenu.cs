using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isPaused;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    private void Toggle()
    {
        _isPaused = !_isPaused;
        pausePanel.SetActive(_isPaused);
        Time.timeScale       = _isPaused ? 0f : 1f;
        Cursor.lockState     = _isPaused ? CursorLockMode.None   : CursorLockMode.Locked;
        Cursor.visible       = _isPaused;
    }

    public void OnResumeClicked()
    {
        if (_isPaused) Toggle();
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        GameStateManager.Instance?.ResetForRestart();
        HealthSystem.Instance?.ResetHearts();
        PieceCollectionSystem.Instance?.ResetPieces();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        GameStateManager.Instance?.ResetForRestart();
        HealthSystem.Instance?.ResetHearts();
        PieceCollectionSystem.Instance?.ResetPieces();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
