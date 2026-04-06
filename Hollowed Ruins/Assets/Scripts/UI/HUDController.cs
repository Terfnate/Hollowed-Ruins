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

    private CanvasGroup _warningGroup;

    void Start()
    {
        HealthSystem.Instance.OnHeartsChanged.AddListener(UpdateHearts);
        PieceCollectionSystem.Instance.OnPieceCollected.AddListener(UpdatePieceCounter);

        if (ghostWarningIcon != null)
            _warningGroup = ghostWarningIcon.GetComponent<CanvasGroup>();

        // Initialise display
        UpdateHearts(3);
        UpdatePieceCounter(0, 5);
        ghostWarningIcon?.SetActive(false);
    }

    void Update()
    {
        UpdateGhostWarning();
    }

    // ─── Hearts ───────────────────────────────────────────────────────────────

    void UpdateHearts(int current)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null) continue;
            heartIcons[i].sprite = i < current ? heartFullSprite : heartEmptySprite;
        }
    }

    // ─── Piece Counter ────────────────────────────────────────────────────────

    void UpdatePieceCounter(int collected, int total)
    {
        if (pieceCounterText != null)
            pieceCounterText.text = $"{collected} / {total}";
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
