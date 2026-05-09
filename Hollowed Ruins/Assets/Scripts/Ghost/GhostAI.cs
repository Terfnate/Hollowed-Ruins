using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI : MonoBehaviour
{
    public enum GhostState { Patrol, Chase, Stun }

    [Header("Detection")]
    [SerializeField] private float sightRange = 8f;
    [SerializeField] private float sightAngle = 90f;
    [SerializeField] private LayerMask sightBlockMask;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 4f;
    [SerializeField] private float chaseSpeed  = 8f;

    [Header("Patrol")]
    [SerializeField] private float patrolWaitTime = 1.5f;
    [SerializeField] private float patrolRadius   = 10f;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;      // Assign AudioSource on Ghost prefab
    [SerializeField] private AudioClip ghostAlertClip;     // Assign Ghost_Alert
    [SerializeField] private AudioClip ghostRoarClip;      // Assign Ghost_Roar

    public GhostState CurrentState { get; private set; } = GhostState.Patrol;

    private NavMeshAgent _agent;
    private Transform _player;

    private float _stunTimer;
    private float _patrolWaitTimer;
    private Vector3 _patrolTarget;
    private bool _patrolWaiting;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.enabled = false;
    }

    void Start()
    {
        var cfg = GameStateManager.Instance?.levelConfig;
        if (cfg != null)
        {
            patrolSpeed = cfg.ghostPatrolSpeed;
            chaseSpeed  = cfg.ghostChaseSpeed;
        }

        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (NoiseSystem.Instance != null)
            NoiseSystem.Instance.OnNoiseEmitted += OnNoiseHeard;

        StartCoroutine(WaitForNavMesh());

        // Start periodic roar
        if (audioSource != null && ghostRoarClip != null)
            StartCoroutine(RoarRoutine());
    }

    IEnumerator WaitForNavMesh()
    {
        yield return null;

        _agent.enabled = true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            _agent.Warp(hit.position);

        float timeout = 5f;
        while (!_agent.isOnNavMesh && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!_agent.isOnNavMesh) yield break;

        SetState(GhostState.Patrol);
        PickNewPatrolTarget();
    }

    void OnDestroy()
    {
        if (NoiseSystem.Instance != null)
            NoiseSystem.Instance.OnNoiseEmitted -= OnNoiseHeard;
    }

    void Update()
    {
        if (!GameStateManager.Instance.IsExploring()) return;

        switch (CurrentState)
        {
            case GhostState.Patrol: UpdatePatrol(); break;
            case GhostState.Chase:  UpdateChase();  break;
            case GhostState.Stun:   UpdateStun();   break;
        }
    }

    // ─── States ───────────────────────────────────────────────────────────────

    void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            SetState(GhostState.Chase);
            PlayGhostAlert();   // NEW: play alert when chase begins
            return;
        }

        if (_patrolWaiting)
        {
            _patrolWaitTimer -= Time.deltaTime;
            if (_patrolWaitTimer <= 0f)
            {
                _patrolWaiting = false;
                PickNewPatrolTarget();
            }
            return;
        }

        if (_agent.isOnNavMesh && !_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _patrolWaiting = true;
            _patrolWaitTimer = patrolWaitTime;
        }
    }

    void UpdateChase()
    {
        if (_player == null) return;

        _agent.SetDestination(_player.position);

        if (!CanSeePlayer() && Vector3.Distance(transform.position, _player.position) > sightRange * 1.5f)
        {
            SetState(GhostState.Patrol);
            PickNewPatrolTarget();
        }
    }

    void UpdateStun()
    {
        _stunTimer -= Time.deltaTime;
        if (_stunTimer <= 0f)
            SetState(GhostState.Patrol);
    }

    // ─── Detection ────────────────────────────────────────────────────────────

    bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 toPlayer = _player.position - transform.position;
        float distance   = toPlayer.magnitude;

        if (distance > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > sightAngle) return false;

        if (Physics.Raycast(transform.position + Vector3.up, toPlayer.normalized, distance, sightBlockMask))
            return false;

        return true;
    }

    void OnNoiseHeard(Vector3 noisePosition, float noiseRadius)
    {
        if (CurrentState == GhostState.Stun) return;

        float dist = Vector3.Distance(transform.position, noisePosition);
        if (dist > noiseRadius) return;

        if (CurrentState == GhostState.Chase)
            SetState(GhostState.Patrol);

        _agent.SetDestination(noisePosition);
        _patrolWaiting = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (CurrentState == GhostState.Stun) return;
        if (!other.CompareTag("Player")) return;

        GameStateManager.Instance.SetState(GameState.ChessDuel);
    }

    public void Stun()
    {
        _stunTimer = stunDuration;
        SetState(GhostState.Stun);
    }

    public void Vanish(float duration)
    {
        StartCoroutine(VanishRoutine(duration));
    }

    IEnumerator VanishRoutine(float duration)
    {
        SetState(GhostState.Stun);

        var renderers  = GetComponentsInChildren<Renderer>();
        var colliders  = GetComponentsInChildren<Collider>();
        foreach (var r in renderers) r.enabled = false;
        foreach (var c in colliders) c.enabled = false;

        yield return new WaitForSeconds(duration);

        if (MazeGenerator.Instance != null)
        {
            Vector3 pos = MazeGenerator.Instance.GetRandomCorridorWorld();
            if (_agent.isOnNavMesh)
                _agent.Warp(pos);
            else
                transform.position = pos;
        }

        foreach (var r in renderers) r.enabled = true;
        foreach (var c in colliders) c.enabled = true;

        SetState(GhostState.Patrol);
        PickNewPatrolTarget();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    void SetState(GhostState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GhostState.Patrol:
                _agent.speed = patrolSpeed;
                _agent.isStopped = false;
                break;
            case GhostState.Chase:
                _agent.speed = chaseSpeed;
                _agent.isStopped = false;
                break;
            case GhostState.Stun:
                _agent.isStopped = true;
                break;
        }
    }

    void PickNewPatrolTarget()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius;
        randomDir += transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            _patrolTarget = hit.position;
            _agent.SetDestination(_patrolTarget);
        }
    }

    // ─── Audio ────────────────────────────────────────────────────────────────

    void PlayGhostAlert()
    {
        if (audioSource != null && ghostAlertClip != null)
            audioSource.PlayOneShot(ghostAlertClip);
    }

    IEnumerator RoarRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
            if (audioSource != null && ghostRoarClip != null)
                audioSource.PlayOneShot(ghostRoarClip);
        }
    }
}
