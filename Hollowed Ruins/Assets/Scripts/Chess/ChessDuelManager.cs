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
    [SerializeField] private float ghostMoveDelay = 1.2f;  // seconds ghost "thinks"

    // Events for the UI to listen to
    public event System.Action<ChessBoard>           OnDuelStarted;
    public event System.Action<ChessPiece, Vector2Int> OnPlayerMoved;
    public event System.Action<ChessPiece, Vector2Int> OnGhostMoved;
    public event System.Action<int>                  OnTurnsRemainingChanged;
    public UnityEvent OnPlayerWon;
    public UnityEvent OnPlayerLost;

    private ChessBoard             _board;
    private GhostChessAI           _ghostAI;
    private ChessObjectiveEvaluator _evaluator;
    private ChessScenario          _currentScenario;

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
            StartDuel();
        else
            _duelActive = false;
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
    }

    // ─── Player Input ─────────────────────────────────────────────────────────

    // Called by ChessBoardUI when the player clicks a cell.
    public void OnCellClicked(Vector2Int cell)
    {
        if (!_duelActive || !_playerTurn) return;

        ChessPiece clicked = _board.GetAt(cell);

        // Select own piece
        if (clicked != null && clicked.Color == PieceColor.White)
        {
            _selectedPiece = clicked;
            _selectedMoves = _board.GetLegalMoves(clicked);
            return;
        }

        // Execute move
        if (_selectedPiece != null && _selectedMoves.Contains(cell))
        {
            ChessPiece captured = _board.ExecuteMove(_selectedPiece, cell);
            OnPlayerMoved?.Invoke(_selectedPiece, cell);

            _selectedPiece = null;
            _selectedMoves.Clear();
            _playerTurn = false;

            ObjectiveResult result = _evaluator.Evaluate(_board, captured);
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
            // Deselect
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
            // Ghost has no moves — player wins
            ResolveDuel(ObjectiveResult.PlayerWin);
            yield break;
        }

        ChessPiece captured = _board.ExecuteMove(piece, to);
        OnGhostMoved?.Invoke(piece, to);

        ObjectiveResult result = _evaluator.Evaluate(_board, captured);
        OnTurnsRemainingChanged?.Invoke(_evaluator.TurnsRemaining);

        if (result != ObjectiveResult.Ongoing)
            ResolveDuel(result);
        else
            _playerTurn = true;
    }

    // ─── Resolution ───────────────────────────────────────────────────────────

    void ResolveDuel(ObjectiveResult result)
    {
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
            // Only resume if still alive
            if (GameStateManager.Instance.CurrentState != GameState.GameOver)
                StartCoroutine(ResumeAfterLoss());
        }
    }

    IEnumerator ResumeAfterWin()
    {
        // Give time for scream/stun animation to play
        yield return new WaitForSecondsRealtime(1.5f);

        GhostAI ghost = FindFirstObjectByType<GhostAI>();
        ghost?.Stun();

        GhostAnimator anim = FindFirstObjectByType<GhostAnimator>();
        anim?.TriggerScream();

        GameStateManager.Instance.SetState(GameState.Exploring);
    }

    IEnumerator ResumeAfterLoss()
    {
        yield return new WaitForSecondsRealtime(1f);
        GameStateManager.Instance.SetState(GameState.Exploring);
    }
}
