using System;
using UnityEngine;
using UnityEngine.AI;

public class Movimentar_NPC : MonoBehaviour
{
    [Header("Waypoints e Missão")]
    public Transform[] waypoints;
    public QuestData questTrigger;

    [Header("Configurações de Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float runAfterSeconds = 5f;

    private int currentWaypointIndex = 0;
    public bool isMoving = false;
    private bool isRunning = false;
    private float movementStartTime;

    private Animator animator;
    private NavMeshAgent agent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ConfigureAgent();
    }

    private void Update()
    {
        if (!isMoving && questTrigger != null && questTrigger.isCompleted)
        {
            StartMovement();
        }

        if (isMoving)
        {
            UpdateMovement();
        }
    }

    private void ConfigureAgent()
    {
        agent.autoBraking = false;
        agent.acceleration = 999f;
        agent.angularSpeed = 999f;
        agent.stoppingDistance = 0.01f;
    }

    private void StartMovement()
    {
        isMoving = true;
        isRunning = false;
        currentWaypointIndex = 0;
        movementStartTime = Time.time;
        agent.speed = walkSpeed;
        agent.isStopped = false;
        SetDestinationToCurrentWaypoint();
    }

    private void UpdateMovement()
    {
        UpdateSpeedAndAnimation();

        // Força a velocidade constante para evitar desaceleração
        agent.velocity = agent.desiredVelocity.normalized * agent.speed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                ProceedToNextWaypoint();
            }
        }
    }

    private void UpdateSpeedAndAnimation()
    {
        if (!isRunning && Time.time - movementStartTime >= runAfterSeconds)
        {
            isRunning = true;
            agent.speed = runSpeed;
        }

        if (isRunning)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true); // força a animação de corrida
        }
        else
        {
            bool isMovingNow = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isWalking", isMovingNow);
            animator.SetBool("isRunning", false);
        }

    }

    private void ProceedToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length)
        {
            StopMovement();
        }
        else
        {
            SetDestinationToCurrentWaypoint();
        }
    }

    private void StopMovement()
    {
        isMoving = false;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
    }

    private void SetDestinationToCurrentWaypoint()
    {
        if (waypoints != null && currentWaypointIndex < waypoints.Length)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

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
