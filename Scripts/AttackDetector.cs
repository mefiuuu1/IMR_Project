using UnityEngine;
using UnityEngine.Events;

public class AttackDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float minSwingSpeed = 3f;
    public float attackCooldown = 0.5f;
    public float attackDuration = 0.3f;

    [Header("Events")]
    public UnityEvent<int> onAttackDetected;

    [Header("Debug")]
    public bool showDebug = true;

    private Vector3 lastPosition;
    private Vector3 velocity;
    private float cooldownTimer = 0f;
    private float attackTimer = 0f;
    private bool isHeld = false;

    [HideInInspector] public int lastAttackDirection = -1;
    [HideInInspector] public bool attackInProgress = false;
    [HideInInspector] public float attackSpeed = 0f;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        cooldownTimer -= Time.deltaTime;

        if (attackInProgress)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                attackInProgress = false;
                if (showDebug) Debug.Log("Attack window closed");
            }
        }

        if (isHeld && cooldownTimer <= 0 && !attackInProgress)
        {
            DetectSwingDirection();
        }
    }

    void DetectSwingDirection()
    {
        float speed = velocity.magnitude;

        if (speed < minSwingSpeed)
            return;

        Vector3 dir = velocity.normalized;
        int attackDir = -1;

        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x) && dir.y < -0.3f)
        {
            attackDir = 0;
            if (showDebug) Debug.Log($">>> ATAC: SUS (speed: {speed:F1})");
        }
        else if (dir.x < -0.5f)
        {
            attackDir = 1;
            if (showDebug) Debug.Log($">>> ATAC: STANGA (speed: {speed:F1})");
        }
        else if (dir.x > 0.5f)
        {
            attackDir = 2;
            if (showDebug) Debug.Log($">>> ATAC: DREAPTA (speed: {speed:F1})");
        }

        if (attackDir != -1)
        {
            lastAttackDirection = attackDir;
            attackInProgress = true;
            attackSpeed = speed;
            cooldownTimer = attackCooldown;
            attackTimer = attackDuration;

            onAttackDetected?.Invoke(attackDir);
        }
    }

    public void OnGrab()
    {
        isHeld = true;
        ResetAttack();
        if (showDebug) Debug.Log("=== Sabie luata ===");
    }

    public void OnRelease()
    {
        isHeld = false;
        ResetAttack();
        if (showDebug) Debug.Log("=== Sabie lasata ===");
    }

    public void ResetAttack()
    {
        lastAttackDirection = -1;
        attackInProgress = false;
        attackSpeed = 0f;
        attackTimer = 0f;
    }
}