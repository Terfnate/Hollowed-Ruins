using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GhostAI : MonoBehaviour
{
    public enum GhostState { Patrol, Chase, Stun }

    [Header("Detection")]
    [SerializeField] private float sightRange = 8f;
    [SerializeField] private float sightAngle = 90f;   // half-angle of view cone
    [SerializeField] private LayerMask sightBlockMask;  // walls layer

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float chaseSpeed  = 5f;

    [Header("Patrol")]
    [SerializeField] private float patrolWaitTime = 1.5f;  // pause at each waypoint
    [SerializeField] private float patrolRadius   = 10f;   // random wander radius

    [Header("Stun")]
    [SerializeField] private float stunDuration = 3f;

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
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        NoiseSystem.Instance.OnNoiseEmitted.AddListener(OnNoiseHeard);

        SetState(GhostState.Patrol);
        PickNewPatrolTarget();
    }

    void OnDestroy()
    {
        if (NoiseSystem.Instance != null)
            NoiseSystem.Instance.OnNoiseEmitted.RemoveListener(OnNoiseHeard);
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

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _patrolWaiting = true;
            _patrolWaitTimer = patrolWaitTime;
        }
    }

    void UpdateChase()
    {
        if (_player == null) return;

        _agent.SetDestination(_player.position);

        // Lost sight and player is far — go back to patrol
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

        // Raycast to check for walls blocking sight
        if (Physics.Raycast(transform.position + Vector3.up, toPlayer.normalized, distance, sightBlockMask))
            return false;

        return true;
    }

    void OnNoiseHeard(Vector3 noisePosition, float noiseRadius)
    {
        if (CurrentState == GhostState.Stun) return;

        float dist = Vector3.Distance(transform.position, noisePosition);
        if (dist <= noiseRadius)
        {
            // Investigate noise source
            if (CurrentState == GhostState.Patrol)
            {
                _agent.SetDestination(noisePosition);
                _patrolWaiting = false;
            }
            // If already chasing, noise doesn't override
        }
    }

    // ─── Catch Player ─────────────────────────────────────────────────────────

    void OnTriggerEnter(Collider other)
    {
        if (CurrentState == GhostState.Stun) return;
        if (!other.CompareTag("Player")) return;

        GameStateManager.Instance.SetState(GameState.ChessDuel);
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void Stun()
    {
        _stunTimer = stunDuration;
        SetState(GhostState.Stun);
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
}
