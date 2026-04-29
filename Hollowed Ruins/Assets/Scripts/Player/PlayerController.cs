using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float cameraSensitivity = 2f;
    [SerializeField] private float cameraMinY = -20f;
    [SerializeField] private float cameraMaxY = 60f;

    [Header("Noise")]
    [SerializeField] private float walkNoiseRadius = 5f;
    [SerializeField] private float runNoiseRadius = 12f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepSource;   // Assign FootSteps_Run clip here

    [Header("Last Heart Mode")]
    [SerializeField] private float lastHeartSpeedMultiplier = 1.3f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 30f;

    private CharacterController _controller;
    private PlayerInput _input;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _runAction;

    private Vector3 _velocity;
    private float _cameraYaw;
    private float _cameraPitch;

    private bool _isMoving;
    private bool _isRunning;
    private bool _lastHeartMode;
    private bool _isDashing;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private Vector3 _dashDirection;

    public float DashCooldownRemaining => _dashCooldownTimer;
    public bool DashReady => _lastHeartMode && !_isDashing && _dashCooldownTimer <= 0f;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInput>();
        _moveAction = _input.actions["Move"];
        _lookAction = _input.actions["Look"];
        _runAction = _input.actions["Sprint"];
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameStateManager.Instance.OnStateChanged.AddListener(OnStateChanged);
        HealthSystem.Instance.OnHeartsChanged.AddListener(OnHeartsChanged);
    }

    void OnDestroy()
    {
        if (HealthSystem.Instance != null)
            HealthSystem.Instance.OnHeartsChanged.RemoveListener(OnHeartsChanged);
    }

    void OnHeartsChanged(int hearts)
    {
        _lastHeartMode = hearts == 1;
    }

    void OnStateChanged(GameState state)
    {
        bool exploring = state == GameState.Exploring;
        Cursor.lockState = exploring ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !exploring;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            GameStateManager.Instance.SetState(GameState.ChessDuel);

        if (Input.GetKeyDown(KeyCode.L))
            HealthSystem.Instance.LoseHeart();

        if (!GameStateManager.Instance.IsExploring()) return;

        HandleMovement();
        HandleCamera();
        HandleNoise();
        HandleAnimation();
        HandleFootsteps();   // NEW
    }

    void HandleMovement()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        _isRunning = _runAction.IsPressed();
        _isMoving = moveInput.sqrMagnitude > 0.01f;

        // Tick cooldown
        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.deltaTime;

        // Initiate dash (Q) — last heart only, grounded or airborne
        if (_lastHeartMode && !_isDashing && _dashCooldownTimer <= 0f &&
            Keyboard.current.qKey.wasPressedThisFrame)
        {
            Vector3 dir = Quaternion.Euler(0f, _cameraYaw, 0f) * new Vector3(moveInput.x, 0f, moveInput.y);
            _dashDirection = dir.sqrMagnitude > 0.01f ? dir.normalized : transform.forward;
            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCooldownTimer = dashCooldown;
        }

        // Resolve horizontal movement
        Vector3 move;
        float speed;

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f) _isDashing = false;
            move  = _dashDirection;
            speed = dashSpeed;
        }
        else
        {
            move  = Quaternion.Euler(0f, _cameraYaw, 0f) * new Vector3(moveInput.x, 0f, moveInput.y);
            speed = _isRunning ? runSpeed : walkSpeed;
            if (_lastHeartMode && _isRunning)
                speed *= lastHeartSpeedMultiplier;
        }

        // Vertical
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        if (_lastHeartMode && _controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            _velocity.y = jumpForce;

        _velocity.y += gravity * Time.deltaTime;

        _controller.Move((move * speed + _velocity) * Time.deltaTime);

        if (!_isDashing && move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleCamera()
    {
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();

        _cameraYaw   += lookInput.x * cameraSensitivity;
        _cameraPitch -= lookInput.y * cameraSensitivity;
        _cameraPitch  = Mathf.Clamp(_cameraPitch, cameraMinY, cameraMaxY);

        if (cameraTarget != null)
            cameraTarget.rotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0f);
    }

    void HandleNoise()
    {
        if (!_isMoving) return;

        float radius = _isRunning ? runNoiseRadius : walkNoiseRadius;
        NoiseSystem.Instance?.EmitNoise(transform.position, radius);
    }

    void HandleAnimation()
    {
        if (animator == null) return;

        float currentSpeed = 0f;
        if (_isMoving)
        {
            currentSpeed = _isRunning ? runSpeed : walkSpeed;
            if (_lastHeartMode && _isRunning)
                currentSpeed *= lastHeartSpeedMultiplier;
        }
        animator.SetFloat("Speed", currentSpeed);
    }

    void HandleFootsteps()
    {
        if (footstepSource == null) return;

        if (_isMoving)
        {
            // Adjust pitch based on running vs walking
            footstepSource.pitch = _isRunning ? 1.0f : 0.7f;

            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            footstepSource.Stop();
        }
    }

    public void EmitLoudNoise(float radius)
    {
        NoiseSystem.Instance?.EmitNoise(transform.position, radius);
    }
}
