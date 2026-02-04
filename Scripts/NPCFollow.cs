using UnityEngine;
using UnityEngine.AI;

public class NPCFollow : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform playerTarget;

    public float stoppingDistance = 3f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = stoppingDistance;

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerTarget = player.transform;
            }
        }
    }

    void Update()
    {
        if (playerTarget != null)
        {

            agent.SetDestination(playerTarget.position);
        }
    }
}