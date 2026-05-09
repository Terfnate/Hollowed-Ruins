using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Exploring,
    ChessDuel,
    GameOver,
    Win
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Level Config")]
    [SerializeField] public LevelConfig levelConfig;

    public GameState CurrentState { get; private set; } = GameState.Exploring;

    public UnityEvent<GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Pass this scene's LevelConfig to the persisted instance before destroying
            if (levelConfig != null)
                Instance.levelConfig = levelConfig;
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Exploring:
                Time.timeScale = 1f;
                break;
            case GameState.ChessDuel:
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.Win:
                Time.timeScale = 0f;
                break;
        }
    }

    public bool IsExploring() => CurrentState == GameState.Exploring;
    public bool IsInChessDuel() => CurrentState == GameState.ChessDuel;

    public void ResetForRestart()
    {
        CurrentState = GameState.Exploring;
        Time.timeScale = 1f;
    }
}
