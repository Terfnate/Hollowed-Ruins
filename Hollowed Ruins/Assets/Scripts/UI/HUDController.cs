using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the HUD panel.
// Displays hearts and piece collection progress during exploration.
public class HUDController : MonoBehaviour
{
    [Header("Hearts")]
    [SerializeField] private Image[] heartIcons;        // 3 heart Image components
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;

    [Header("Piece Counter")]
    [SerializeField] private TextMeshProUGUI pieceCounterText;  // e.g. "2 / 5"

    [Header("Ghost Warning")]
    [SerializeField] private GameObject ghostWarningIcon;       // shown when ghost is chasing
    [SerializeField] private float warningPulseSpeed = 3f;

    [Header("Last Heart Overlay")]
    [SerializeField] private Image lastHeartOverlay;            // full-screen red Image, color set in Inspector
    [SerializeField] private float heartbeatSpeed = 1.8f;       // ~108 BPM at 1.8
    [SerializeField] private float overlayMinAlpha = 0f;
    [SerializeField] private float overlayMaxAlpha = 0.4f;

    [Header("Dash Cooldown")]
    [SerializeField] private GameObject dashIndicator;          // parent object shown only in last-heart mode
    [SerializeField] private Image dashCooldownFill;            // Image (Filled, radial) tracks cooldown
    [SerializeField] private TextMeshProUGUI dashCooldownText;  // optional "DASH  3s" label

    private CanvasGroup _warningGroup;
    private bool _lastHeartActive;
    private PlayerController _player;

    void Start()
    {
        if (HealthSystem.Instance != null)
            HealthSystem.Instance.OnHeartsChanged.AddListener(UpdateHearts);

        if (PieceCollectionSystem.Instance != null)
            PieceCollectionSystem.Instance.OnPieceCollected += UpdatePieceCounter;

        if (ghostWarningIcon != null)
            _warningGroup = ghostWarningIcon.GetComponent<CanvasGroup>();

        // Initialise display
        UpdateHearts(3);
        UpdatePieceCounter(0, 5);
        ghostWarningIcon?.SetActive(false);

        if (lastHeartOverlay != null)
            lastHeartOverlay.gameObject.SetActive(false);

        _player = FindFirstObjectByType<PlayerController>();
        dashIndicator?.SetActive(false);
    }

    void Update()
    {
        UpdateGhostWarning();
        UpdateLastHeartOverlay();
        UpdateDashIndicator();
    }

    // ─── Hearts ───────────────────────────────────────────────────────────────

    void UpdateHearts(int current)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null) continue;
            heartIcons[i].sprite = i < current ? heartFullSprite : heartEmptySprite;
        }

        _lastHeartActive = current == 1;
        if (lastHeartOverlay != null)
            lastHeartOverlay.gameObject.SetActive(_lastHeartActive);
        dashIndicator?.SetActive(_lastHeartActive);
    }

    // ─── Piece Counter ────────────────────────────────────────────────────────

    void UpdatePieceCounter(int collected, int total)
    {
        if (pieceCounterText != null)
            pieceCounterText.text = $"{collected} / {total}";
    }

    // ─── Last Heart Overlay ───────────────────────────────────────────────────

    void UpdateLastHeartOverlay()
    {
        if (lastHeartOverlay == null || !_lastHeartActive) return;

        // Two sharp peaks close together, then a pause — mimics a heartbeat (lub-dub)
        float t = (Time.time * heartbeatSpeed) % 1f;
        float beat = Mathf.Max(
            Mathf.Exp(-((t - 0.15f) * (t - 0.15f)) / 0.004f),   // first beat
            Mathf.Exp(-((t - 0.30f) * (t - 0.30f)) / 0.004f)    // second beat
        );

        Color c = lastHeartOverlay.color;
        c.a = Mathf.Lerp(overlayMinAlpha, overlayMaxAlpha, beat);
        lastHeartOverlay.color = c;
    }

    // ─── Dash Indicator ───────────────────────────────────────────────────────

    void UpdateDashIndicator()
    {
        if (!_lastHeartActive || _player == null) return;

        float remaining = _player.DashCooldownRemaining;
        bool ready = _player.DashReady;

        if (dashCooldownFill != null)
            // Fill goes 0 → 1 as the cooldown counts down (empty while charging, full when ready)
            dashCooldownFill.fillAmount = ready ? 1f : 1f - (remaining / 30f);

        if (dashCooldownText != null)
            dashCooldownText.text = ready ? "DASH  [Q]" : $"DASH  {Mathf.CeilToInt(remaining)}s";
    }

    // ─── Ghost Warning ────────────────────────────────────────────────────────

    void UpdateGhostWarning()
    {
        if (ghostWarningIcon == null) return;

        GhostAI ghost = FindFirstObjectByType<GhostAI>();
        if (ghost == null) return;

        bool chasing = ghost.CurrentState == GhostAI.GhostState.Chase;
        ghostWarningIcon.SetActive(chasing);

        // Pulse the icon alpha
        if (chasing && _warningGroup != null)
            _warningGroup.alpha = Mathf.Abs(Mathf.Sin(Time.time * warningPulseSpeed));
    }
}
