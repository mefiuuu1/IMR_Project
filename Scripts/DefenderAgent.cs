using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DefenderAgent : Agent
{
    [Header("References")]
    public AttackDetector playerSword;
    public Transform player;
    public Animator animator;

    [Header("Block Settings")]
    public float blockDuration = 0.5f;
    public float reactionWindow = 0.8f;

    [Header("Mode")]
    public bool useNeuralNetwork = false;
    public bool trainingMode = false;
    public float episodeTimeout = 10f;

    [Header("Debug")]
    public bool showDebug = true;

    private int currentBlockDirection = -1;
    private float blockTimer = 0f;
    private float reactionTimer = 0f;
    private float episodeTimer = 0f;
    private bool waitingForBlock = false;
    private int attackToBlock = -1;
    private bool attackProcessed = false;

    private int totalAttacks = 0;
    private int successfulBlocks = 0;

    public override void Initialize()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public override void OnEpisodeBegin()
    {
        currentBlockDirection = -1;
        blockTimer = 0f;
        reactionTimer = 0f;
        episodeTimer = 0f;
        waitingForBlock = false;
        attackToBlock = -1;
        attackProcessed = false;

        if (playerSword != null)
            playerSword.ResetAttack();

        if (showDebug && totalAttacks > 0)
        {
            float accuracy = (float)successfulBlocks / totalAttacks * 100f;
            Debug.Log($"Episode ended. Accuracy: {accuracy:F1}% ({successfulBlocks}/{totalAttacks})");
        }

        totalAttacks = 0;
        successfulBlocks = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        int attackDir = (playerSword != null && playerSword.attackInProgress) ? playerSword.lastAttackDirection : -1;

        sensor.AddObservation(attackDir == 0 ? 1f : 0f);
        sensor.AddObservation(attackDir == 1 ? 1f : 0f);
        sensor.AddObservation(attackDir == 2 ? 1f : 0f);
        sensor.AddObservation(playerSword != null && playerSword.attackInProgress ? 1f : 0f);

        float speed = (playerSword != null && playerSword.attackInProgress) ? playerSword.attackSpeed / 10f : 0f;
        sensor.AddObservation(Mathf.Clamp01(speed));

        sensor.AddObservation(currentBlockDirection == 0 ? 1f : 0f);
        sensor.AddObservation(currentBlockDirection == 1 ? 1f : 0f);
        sensor.AddObservation(currentBlockDirection == 2 ? 1f : 0f);
        sensor.AddObservation(waitingForBlock ? reactionTimer / reactionWindow : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (playerSword != null && playerSword.attackInProgress && !attackProcessed)
        {
            attackToBlock = playerSword.lastAttackDirection;
            waitingForBlock = true;
            reactionTimer = reactionWindow;
            attackProcessed = true;
            totalAttacks++;

            if (showDebug)
                Debug.Log($">>> ATAC DETECTAT: {GetDirectionName(attackToBlock)}");


            if (!useNeuralNetwork && currentBlockDirection == -1)
            {
                currentBlockDirection = attackToBlock;
                blockTimer = blockDuration;
                PlayBlockAnimation(currentBlockDirection);

                if (showDebug)
                    Debug.Log($"[REACTIVE] Agent BLOCHEAZA: {GetDirectionName(currentBlockDirection)}");
            }
        }

        if (playerSword != null && !playerSword.attackInProgress)
        {
            attackProcessed = false;
        }

        if (useNeuralNetwork)
        {
            int blockAction = actions.DiscreteActions[0];

            if (blockAction > 0 && currentBlockDirection == -1 && waitingForBlock)
            {
                currentBlockDirection = blockAction - 1;
                blockTimer = blockDuration;
                PlayBlockAnimation(currentBlockDirection);

                if (showDebug)
                    Debug.Log($"[NEURAL] Agent BLOCHEAZA: {GetDirectionName(currentBlockDirection)}");
            }
        }

        if (blockTimer > 0)
        {
            blockTimer -= Time.fixedDeltaTime;
            if (blockTimer <= 0)
            {
                currentBlockDirection = -1;
            }
        }

        if (waitingForBlock)
        {
            reactionTimer -= Time.fixedDeltaTime;

            if (currentBlockDirection == attackToBlock)
            {
                if (trainingMode) AddReward(1.0f);
                successfulBlocks++;
                waitingForBlock = false;

                if (showDebug)
                    Debug.Log($"*** BLOCK REUSIT! *** ({GetDirectionName(attackToBlock)})");
            }
            else if (reactionTimer <= 0)
            {
                if (trainingMode) AddReward(-1.0f);
                waitingForBlock = false;

                if (showDebug)
                    Debug.Log($"!!! BLOCK RATAT !!! (trebuia {GetDirectionName(attackToBlock)})");
            }
        }

        if (trainingMode)
        {
            AddReward(-0.001f);
            episodeTimer += Time.fixedDeltaTime;
            if (episodeTimer >= episodeTimeout)
                EndEpisode();
        }
    }

    void PlayBlockAnimation(int direction)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is NULL!");
            return;
        }

        animator.ResetTrigger("BlockUp");
        animator.ResetTrigger("BlockLeft");
        animator.ResetTrigger("BlockRight");

        string triggerName = "";
        switch (direction)
        {
            case 0: triggerName = "BlockUp"; break;
            case 1: triggerName = "BlockLeft"; break;
            case 2: triggerName = "BlockRight"; break;
        }

        if (!string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
            if (showDebug)
                Debug.Log($"Animation trigger: {triggerName}");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            discreteActions[0] = 3;
    }

    string GetDirectionName(int dir)
    {
        switch (dir)
        {
            case 0: return "SUS";
            case 1: return "STANGA";
            case 2: return "DREAPTA";
            default: return "NONE";
        }
    }

    public void SimulateAttack(int direction)
    {
        if (playerSword != null)
        {
            playerSword.lastAttackDirection = direction;
            playerSword.attackInProgress = true;
            playerSword.attackSpeed = 5f;
        }
    }
}