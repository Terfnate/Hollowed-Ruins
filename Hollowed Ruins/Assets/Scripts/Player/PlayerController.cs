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

        float speed = _isRunning ? runSpeed : walkSpeed;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = Quaternion.Euler(0f, _cameraYaw, 0f) * move;

        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;

        _controller.Move((move * speed + _velocity) * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
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

        float currentSpeed = _isMoving ? (_isRunning ? runSpeed : walkSpeed) : 0f;
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
