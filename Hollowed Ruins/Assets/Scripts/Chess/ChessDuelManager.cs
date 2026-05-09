using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

// Orchestrates the full chess duel flow.
// Attach to a persistent manager GameObject alongside GameStateManager.
public class ChessDuelManager : MonoBehaviour
{
    public static ChessDuelManager Instance { get; private set; }

    [Header("Scenarios")]
    [SerializeField] private List<ChessScenario> scenarios;

    [Header("Ghost AI Delay")]
    [SerializeField] private float ghostMoveDelay     = 1.2f;
    [SerializeField] private float ghostVanishDuration = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource chessAudioSource;   // assign AudioSource for chess duel
    [SerializeField] private AudioClip chessClip;            // assign Chess_duel clip
    [SerializeField] private float fadeDuration = 1.5f;      // seconds for fade in

    // Events for the UI to listen to
    public event System.Action<ChessBoard> OnDuelStarted;
    public event System.Action<ChessPiece, Vector2Int> OnPlayerMoved;
    public event System.Action<ChessPiece, Vector2Int> OnGhostMoved;
    public event System.Action<int> OnTurnsRemainingChanged;
    public UnityEvent OnPlayerWon;
    public UnityEvent OnPlayerLost;

    private ChessBoard _board;
    private GhostChessAI _ghostAI;
    private ChessObjectiveEvaluator _evaluator;
    private ChessScenario _currentScenario;

    private bool _playerTurn = true;
    private bool _duelActive = false;

    private ChessPiece _selectedPiece;
    private List<Vector2Int> _selectedMoves = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.ChessDuel)
        {
            StartCoroutine(StartDuelNextFrame());
        }
        else
        {
            _duelActive = false;
            // When duel ends, fade normal audio back in
            StartCoroutine(FadeInAllAudio());
        }
    }

    IEnumerator StartDuelNextFrame()
    {
        yield return null; // wait for ChessBoardUI.Start() to subscribe
        StartDuel();
    }

    // ─── Duel Flow ────────────────────────────────────────────────────────────

    void StartDuel()
    {
        _board     = new ChessBoard();
        _evaluator = new ChessObjectiveEvaluator();

        // Pick a random scenario
        _currentScenario = scenarios[Random.Range(0, scenarios.Count)];
        _board.LoadScenario(_currentScenario);

        _ghostAI = new GhostChessAI(_board);
        _evaluator.Load(_currentScenario.objective);

        _playerTurn  = true;
        _duelActive  = true;
        _selectedPiece = null;
        _selectedMoves.Clear();

        OnDuelStarted?.Invoke(_board);
        OnTurnsRemainingChanged?.Invoke(_evaluator.TurnsRemaining);

        // Play chess duel audio with fade in and looping
        if (chessAudioSource != null && chessClip != null)
        {
            chessAudioSource.clip = chessClip;
            chessAudioSource.loop = true;   // keep looping until duel ends
            chessAudioSource.volume = 0f;
            chessAudioSource.Play();
            StartCoroutine(FadeInSource(chessAudioSource));
        }
    }

    // ─── Fade Helpers ─────────────────────────────────────────────────────────

    IEnumerator FadeInAllAudio()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            AudioListener.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        AudioListener.volume = 1f;
    }

    IEnumerator FadeInSource(AudioSource src)
    {
        if (src == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        src.volume = 1f;
    }

    // ─── Player Input ─────────────────────────────────────────────────────────

    public void OnCellClicked(Vector2Int cell)
    {
        if (!_duelActive || !_playerTurn) return;

        ChessPiece clicked = _board.GetAt(cell);

        if (clicked != null && clicked.Color == PieceColor.White)
        {
            _selectedPiece = clicked;
            _selectedMoves = _board.GetLegalMoves(clicked);
            return;
        }

        if (_selectedPiece != null && _selectedMoves.Contains(cell))
        {
            ChessPiece captured = _board.ExecuteMove(_selectedPiece, cell);
            OnPlayerMoved?.Invoke(_selectedPiece, cell);

            _selectedPiece = null;
            _selectedMoves.Clear();
            _playerTurn = false;

            ObjectiveResult result = _evaluator.EvaluateAfterPlayerMove(_board, captured);
            OnTurnsRemainingChanged?.Invoke(_evaluator.TurnsRemaining);

            if (result != ObjectiveResult.Ongoing)
            {
                ResolveDuel(result);
                return;
            }

            StartCoroutine(GhostTurnRoutine());
        }
        else
        {
            _selectedPiece = null;
            _selectedMoves.Clear();
        }
    }

    public ChessPiece GetSelectedPiece() => _selectedPiece;
    public List<Vector2Int> GetSelectedMoves() => _selectedMoves;
    public string GetCurrentObjectiveDescription() => _currentScenario?.objective.description ?? "";

    // ─── Ghost Turn ───────────────────────────────────────────────────────────

    IEnumerator GhostTurnRoutine()
    {
        yield return new WaitForSecondsRealtime(ghostMoveDelay);

        var (piece, to) = _ghostAI.PickMove();

        if (piece == null)
        {
            ResolveDuel(ObjectiveResult.PlayerWin);
            yield break;
        }

        ChessPiece captured = _board.ExecuteMove(piece, to);
        OnGhostMoved?.Invoke(piece, to);

        ObjectiveResult result = _evaluator.EvaluateAfterGhostMove(_board, captured);
        OnTurnsRemainingChanged?.Invoke(_evaluator.TurnsRemaining);

        if (result != ObjectiveResult.Ongoing)
            ResolveDuel(result);
        else
            _playerTurn = true;
    }

    // ─── Resolution ───────────────────────────────────────────────────────────

    void ResolveDuel(ObjectiveResult result)
    {
        if (!_duelActive) return;
        _duelActive = false;

        if (result == ObjectiveResult.PlayerWin)
        {
            OnPlayerWon?.Invoke();
            StartCoroutine(ResumeAfterWin());
        }
        else
        {
            OnPlayerLost?.Invoke();
            HealthSystem.Instance.LoseHeart();
            if (GameStateManager.Instance.CurrentState != GameState.GameOver)
                StartCoroutine(ResumeAfterLoss());
        }

        // Stop chess duel audio when duel ends
        if (chessAudioSource != null && chessAudioSource.isPlaying)
        {
            chessAudioSource.Stop();
        }
    }

    IEnumerator ResumeAfterWin()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        GhostAI ghost = FindFirstObjectByType<GhostAI>();
        ghost?.Vanish(ghostVanishDuration);

        GhostAnimator anim = FindFirstObjectByType<GhostAnimator>();
        anim?.TriggerScream();

        GameStateManager.Instance.SetState(GameState.Exploring);
    }

    IEnumerator ResumeAfterLoss()
    {
        yield return new WaitForSecondsRealtime(1f);

        GhostAI ghost = FindFirstObjectByType<GhostAI>();
        ghost?.Stun();

        GameStateManager.Instance.SetState(GameState.Exploring);
    }
}
