using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Scene Names")]
    [SerializeField] private string tutorialScene = "Tutorial";
    [SerializeField] private string easyScene = "Easy";
    [SerializeField] private string mediumScene = "Medium";
    [SerializeField] private string hardScene = "Hard";

    private void Start()
    {
        mainPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
    }

    public void OnPlayClicked()
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OnBackClicked()
    {
        levelSelectPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void OnTutorialClicked()  => SceneManager.LoadScene(tutorialScene);
    public void OnEasyClicked()      => SceneManager.LoadScene(easyScene);
    public void OnMediumClicked()    => SceneManager.LoadScene(mediumScene);
    public void OnHardClicked()      => SceneManager.LoadScene(hardScene);

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
