using UnityEngine;

// Drives the ghost's Animator based on GhostAI state.
// Attach alongside GhostAI on the ghost GameObject.
[RequireComponent(typeof(GhostAI))]
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class GhostAnimator : MonoBehaviour
{
    private static readonly int IsMoving  = Animator.StringToHash("IsMoving");
    private static readonly int IsChasing = Animator.StringToHash("IsChasing");
    private static readonly int IsStunned = Animator.StringToHash("IsStunned");
    private static readonly int Scream    = Animator.StringToHash("Scream");
    private static readonly int Speed     = Animator.StringToHash("Speed"); // NEW

    private Animator _animator;
    private GhostAI  _ghost;
    private UnityEngine.AI.NavMeshAgent _agent;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _ghost    = GetComponent<GhostAI>();
        _agent    = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {
        if (_animator == null) return;

        bool stunned = _ghost.CurrentState == GhostAI.GhostState.Stun;
        bool chasing = _ghost.CurrentState == GhostAI.GhostState.Chase;
        bool moving  = _ghost.CurrentState != GhostAI.GhostState.Stun;

        _animator.SetBool(IsMoving,  moving);
        _animator.SetBool(IsChasing, chasing);
        _animator.SetBool(IsStunned, stunned);

        // NEW: Drive Idle/Walk animations based on NavMeshAgent velocity
        float speedValue = _agent.velocity.magnitude;
        _animator.SetFloat(Speed, speedValue);
    }

    public void TriggerScream()
    {
        _animator?.SetTrigger(Scream);
    }
}
