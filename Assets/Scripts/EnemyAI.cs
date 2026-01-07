using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator animator;
    public Transform swordHitPoint;

    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 25;
    public float detectionRange = 15f;

    [Header("Timing")]
    public float waitTimeBeforeStart = 5f;
    public float withdrawSwordDuration = 1.5f;

    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int WithdrawSword = Animator.StringToHash("WithdrawSword");
    private static readonly int SheatheSword = Animator.StringToHash("SheatheSword");

    private enum EnemyState { Waiting, WithdrawingSword, Chasing, Attacking, Sheathing, Idle }
    private EnemyState currentState = EnemyState.Waiting;

    private float stateTimer = 0f;
    private float attackTimer = 0f;
    private bool playerIsDead = false;
    private bool hasDealtDamage = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        
        if (player == null)
        {
            var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                player = xrOrigin.Camera.transform;
            }
            else
            {
                player = Camera.main?.transform;
            }
        }

        stateTimer = waitTimeBeforeStart;
        agent.isStopped = true;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Waiting:
                HandleWaitingState();
                break;

            case EnemyState.WithdrawingSword:
                HandleWithdrawingSwordState();
                break;

            case EnemyState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;

            case EnemyState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;

            case EnemyState.Sheathing:
                HandleSheathingState();
                break;

            case EnemyState.Idle:
                break;
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    void HandleWaitingState()
    {
        stateTimer -= Time.deltaTime;
        
        if (stateTimer <= 0)
        {
            currentState = EnemyState.WithdrawingSword;
            animator.SetTrigger(WithdrawSword);
            stateTimer = withdrawSwordDuration;
        }
    }

    void HandleWithdrawingSwordState()
    {
        stateTimer -= Time.deltaTime;
        
        if (stateTimer <= 0)
        {
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
            animator.SetBool(IsRunning, true);
        }
    }

    void HandleChasingState(float distanceToPlayer)
    {
        if (playerIsDead)
        {
            currentState = EnemyState.Sheathing;
            agent.isStopped = true;
            animator.SetBool(IsRunning, false);
            animator.SetTrigger(SheatheSword);
            return;
        }

        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attacking;
            agent.isStopped = true;
            animator.SetBool(IsRunning, false);
        }
    }

    void HandleAttackingState(float distanceToPlayer)
    {
        if (playerIsDead)
        {
            currentState = EnemyState.Sheathing;
            animator.SetTrigger(SheatheSword);
            return;
        }

        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
        }

        if (distanceToPlayer > attackRange * 1.5f)
        {
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
            animator.SetBool(IsRunning, true);
            return;
        }

        if (attackTimer <= 0)
        {
            animator.SetTrigger(Attack);
            attackTimer = attackCooldown;
            hasDealtDamage = false;
        }
    }

    void HandleSheathingState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Sheathing Sword") && stateInfo.normalizedTime >= 0.9f)
        {
            currentState = EnemyState.Idle;
        }
    }

    public void DealDamage()
    {
        if (hasDealtDamage || playerIsDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange * 1.2f)
        {
            var playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                hasDealtDamage = true;

                if (playerHealth.currentHealth <= 0)
                {
                    playerIsDead = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasDealtDamage && currentState == EnemyState.Attacking)
        {
            var playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                hasDealtDamage = true;

                if (playerHealth.currentHealth <= 0)
                {
                    playerIsDead = true;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
