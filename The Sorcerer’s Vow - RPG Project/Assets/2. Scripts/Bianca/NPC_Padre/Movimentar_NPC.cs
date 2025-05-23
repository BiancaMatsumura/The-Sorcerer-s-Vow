using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Movimentar_NPC : MonoBehaviour
{
    [Header("Waypoints e Missão")]
    public Transform[] waypoints;
    public QuestData questTrigger01;
    public QuestData questTrigger02;
    public QuestData questTrigger03;

    public Transform pontoB;

    [Header("Configurações de Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float runAfterSeconds = 5f;
    public float waypointReachDistance = 1f;

    private int currentWaypointIndex = 0;
    public bool isMoving = false;
    private bool isRunning = false;
    private float movementStartTime;

    private Animator animator;
    private NavMeshAgent agent;

    private bool hasStartedMovement = false;
    private bool isMovingToPontoB = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        ConfigureAgent();
    }

    private void Update()
    {
        if (!isMoving && !hasStartedMovement && questTrigger01 != null && questTrigger01.isCompleted)
        {
            StartMovement();
            hasStartedMovement = true;
        }

        if (isMoving)
        {
            UpdateMovement();
        }

        if (questTrigger03 != null && questTrigger03.isCompleted && !isMovingToPontoB)
        {
            MoveToPontoB();
        }

        if (isMovingToPontoB)
        {
            CheckIfReachedPontoB();
        }

        if (!isMoving && agent != null && agent.enabled && agent.isOnNavMesh && agent.velocity.magnitude > 0.1f)
        {
            Debug.LogWarning($"NPC deveria estar parado mas ainda está se movendo! Velocity: {agent.velocity.magnitude}, isStopped: {agent.isStopped}, hasPath: {agent.hasPath}, pathStatus: {agent.pathStatus}");
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.SetDestination(transform.position);
        }
    }

    private void ConfigureAgent()
    {
        agent.autoBraking = true;
        agent.acceleration = 8f;
        agent.angularSpeed = 120f;
        agent.stoppingDistance = 0.5f;
    }

    private void StartMovement()
    {
        if (agent != null && !agent.enabled)
            agent.enabled = true;

        if (agent != null && agent.isOnNavMesh)
        {
            isMoving = true;
            isRunning = false;
            currentWaypointIndex = 0;
            movementStartTime = Time.time;
            agent.speed = walkSpeed;
            agent.isStopped = false;
            SetDestinationToCurrentWaypoint();
        }
    }

    private void UpdateMovement()
    {
        UpdateSpeedAndAnimation();

        if (HasReachedCurrentWaypoint())
        {
            ProceedToNextWaypoint();
        }
    }

    private bool HasReachedCurrentWaypoint()
    {
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
            return false;

        Transform currentWaypoint = waypoints[currentWaypointIndex];
        if (currentWaypoint == null)
            return false;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);

        bool closeEnough = distanceToWaypoint <= waypointReachDistance;
        bool agentReached = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        bool almostStopped = agent.velocity.magnitude < 0.1f;

        return closeEnough && (agentReached || almostStopped);
    }

    private void UpdateSpeedAndAnimation()
    {
        if (!isRunning && Time.time - movementStartTime >= runAfterSeconds && isMoving)
        {
            isRunning = true;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                agent.speed = runSpeed;
        }

        if (isRunning && isMoving)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
        }
        else
        {
            bool isMovingNow = agent != null && agent.enabled && agent.velocity.magnitude > 0.1f;
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
        isRunning = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.SetDestination(transform.position);
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }

        Debug.Log($"NPC parou no último waypoint: {waypoints[waypoints.Length - 1].name}");

        if (questTrigger02 != null)
        {
            QuestSystem questSystem = FindObjectOfType<QuestSystem>();
            if (questSystem != null)
            {
                questSystem.ActivateQuest(questTrigger02);
            }
        }
    }

    private void SetDestinationToCurrentWaypoint()
    {
        if (waypoints != null && currentWaypointIndex < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint != null && agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(targetWaypoint.position);
                Debug.Log($"NPC indo para waypoint {currentWaypointIndex}: {targetWaypoint.name}");
            }
        }
    }

    private void MoveToPontoB()
    {
        if (agent != null)
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.isStopped = false;
            agent.speed = walkSpeed; // ou runSpeed, se quiser correndo

            agent.SetDestination(pontoB.position);

            isMovingToPontoB = true;

            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);

            Debug.Log("NPC começou a ir para o ponto B...");
        }
    }

    private void CheckIfReachedPontoB()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        float distanceToPontoB = Vector3.Distance(transform.position, pontoB.position);

        if (distanceToPontoB <= agent.stoppingDistance + 0.2f)
        {
            StopAtPontoB();
        }
    }

    private void StopAtPontoB()
    {
        isMovingToPontoB = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            // Se quiser, pode desativar o agente:
            // agent.enabled = false;
        }

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        Debug.Log("NPC chegou ao ponto B!");
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.color = (i == currentWaypointIndex && isMoving) ? Color.green : Color.red;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            if (i == currentWaypointIndex && isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(waypoints[i].position, waypointReachDistance);
            }

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

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.15f);
        }
    }
}
