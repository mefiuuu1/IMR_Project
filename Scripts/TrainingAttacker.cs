using UnityEngine;

public class TrainingAttacker : MonoBehaviour
{
    [Header("References")]
    public DefenderAgent defender;
    public AttackDetector attackDetector;

    [Header("Training Settings")]
    public float minAttackInterval = 0.5f;
    public float maxAttackInterval = 2f;
    public bool autoAttack = true;

    private float attackTimer = 0f;

    void Start()
    {
        if (defender == null)
            defender = FindObjectOfType<DefenderAgent>();

        ResetTimer();
    }

    void Update()
    {
        if (!autoAttack) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            int randomDir = Random.Range(0, 3);
            SimulateAttack(randomDir);
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        attackTimer = Random.Range(minAttackInterval, maxAttackInterval);
    }

    void SimulateAttack(int direction)
    {
        if (attackDetector != null)
        {
            attackDetector.lastAttackDirection = direction;
            attackDetector.attackInProgress = true;
            attackDetector.attackSpeed = Random.Range(3f, 8f);

            Invoke(nameof(ResetAttack), 0.3f);
        }

        Debug.Log($"[Training] Simulated attack: {direction} (0=sus, 1=stanga, 2=dreapta)");
    }

    void ResetAttack()
    {
        if (attackDetector != null)
        {
            attackDetector.attackInProgress = false;
        }
    }

    [ContextMenu("Attack Up")]
    void AttackUp() => SimulateAttack(0);

    [ContextMenu("Attack Left")]
    void AttackLeft() => SimulateAttack(1);

    [ContextMenu("Attack Right")]
    void AttackRight() => SimulateAttack(2);
}