using System;
using UnityEngine;
using UnityEngine.AI;

public class Movimentar_NPC : MonoBehaviour
{
    public Transform[] waypoints;
    public QuestData questTrigger;

    public bool isMoving = false;
    private int currentWaypointIndex = 0;

    private Animator animator;
    private NavMeshAgent agent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.autoBraking = false;
        agent.acceleration = 999f;
        agent.angularSpeed = 999f;
    }

    void Update()
    {
        // Inicia movimentação quando a quest for concluída
        if (!isMoving && questTrigger != null && questTrigger.isCompleted)
        {
            isMoving = true;
            currentWaypointIndex = 0;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        if (!isMoving || waypoints.Length == 0) return;

        // Atualiza animação com base na velocidade
        animator.SetBool("isWalking", agent.velocity.magnitude > 0.1f);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    // NPC chegou ao último waypoint, então ele para
                    isMoving = false;
                    animator.SetBool("isWalking", false);
                    agent.isStopped = true; // Para o NPC
                }
                else
                {
                    // Continua para o próximo waypoint
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                }
            }
        }
    }


    // Gizmos para visualização dos waypoints
    void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);

                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.color = Color.yellow;
                        Vector3 start = waypoints[i].position;
                        Vector3 end = waypoints[i + 1].position;
                        int segments = 10;

                        for (int j = 0; j < segments; j++)
                        {
                            float t1 = j / (float)segments;
                            float t2 = (j + 1) / (float)segments;
                            Vector3 pos1 = Vector3.Lerp(start, end, t1);
                            Vector3 pos2 = Vector3.Lerp(start, end, t2);
                            Gizmos.DrawLine(pos1, pos2);
                        }
                    }
                }
            }
        }
    }
}
