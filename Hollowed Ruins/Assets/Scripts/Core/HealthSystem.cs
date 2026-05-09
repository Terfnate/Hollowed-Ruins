using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }

    [SerializeField] private int maxHearts = 3;

    public int CurrentHearts { get; private set; }

    public UnityEvent<int> OnHeartsChanged;  // passes current hearts
    public UnityEvent OnGameOver;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        CurrentHearts = maxHearts;
    }

    public void ResetHearts()
    {
        CurrentHearts = maxHearts;
        OnHeartsChanged?.Invoke(CurrentHearts);
    }

    public void LoseHeart()
    {
        CurrentHearts--;
        OnHeartsChanged?.Invoke(CurrentHearts);

        if (CurrentHearts <= 0)
        {
            OnGameOver?.Invoke();
            GameStateManager.Instance.SetState(GameState.GameOver);
        }
    }
}
