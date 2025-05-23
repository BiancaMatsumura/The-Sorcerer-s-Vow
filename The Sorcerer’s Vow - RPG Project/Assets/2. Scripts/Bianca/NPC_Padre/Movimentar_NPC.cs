using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Movimentar_NPC : MonoBehaviour
{
    [Header("Waypoints e Missão")]
    public Transform[] waypoints01;
    public Transform[] waypoints02;
    public QuestData questTrigger01;
    public QuestData questTrigger02;
    public QuestData questTrigger03;

    [Header("Configurações de Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float runAfterSeconds = 5f;
    public float waypointReachDistance = 1f;

    private int currentWaypointIndex = 0;
    private int currentWaypointIndex02 = 0;

    private bool isMoving = false;
    private bool isMovingToWaypoints02 = false;
    private bool isRunning = false;

    private bool hasStartedMovement = false;
    private bool hasStartedMovement02 = false;

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
        if (!isMoving && !hasStartedMovement && questTrigger01 != null && questTrigger01.isCompleted)
        {
            StartMovement();
            hasStartedMovement = true;
        }

        if (isMoving)
        {
            UpdateMovement();
        }

        // Inicia movimento para waypoints02 após completar questTrigger03
        if (!isMovingToWaypoints02 && !hasStartedMovement02 && questTrigger03 != null && questTrigger03.isCompleted)
        {
            StartMovement02();
            hasStartedMovement02 = true;
        }

        if (isMovingToWaypoints02)
        {
            UpdateMovement02();
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
        isMoving = true;
        isRunning = false;
        currentWaypointIndex = 0;
        movementStartTime = Time.time;
        agent.speed = walkSpeed;
        agent.isStopped = false;
        SetDestinationToCurrentWaypoint(waypoints01, currentWaypointIndex);
    }

    private void UpdateMovement()
    {
        UpdateSpeedAndAnimation();

        if (HasReachedCurrentWaypoint(waypoints01, currentWaypointIndex))
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints01.Length)
            {
                StopMovement();
            }
            else
            {
                SetDestinationToCurrentWaypoint(waypoints01, currentWaypointIndex);
            }
        }
    }

    private void StopMovement()
    {
        isMoving = false;
        isRunning = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        Debug.Log($"NPC parou no último waypoint01: {waypoints01[waypoints01.Length - 1].name}");

        if (questTrigger02 != null)
        {
            QuestSystem questSystem = FindObjectOfType<QuestSystem>();
            if (questSystem != null)
            {
                questSystem.ActivateQuest(questTrigger02);
            }
        }
    }

    private void StartMovement02()
    {
        isMovingToWaypoints02 = true;
        isRunning = false;
        currentWaypointIndex02 = 0;
        movementStartTime = Time.time;
        agent.speed = walkSpeed;
        agent.isStopped = false;
        SetDestinationToCurrentWaypoint(waypoints02, currentWaypointIndex02);
    }

    private void UpdateMovement02()
    {
        UpdateSpeedAndAnimation();

        if (HasReachedCurrentWaypoint(waypoints02, currentWaypointIndex02))
        {
            currentWaypointIndex02++;

            if (currentWaypointIndex02 >= waypoints02.Length)
            {
                StopMovement02();
            }
            else
            {
                SetDestinationToCurrentWaypoint(waypoints02, currentWaypointIndex02);
            }
        }
    }

    private void StopMovement02()
    {
        isMovingToWaypoints02 = false;
        isRunning = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        Debug.Log($"NPC parou no último waypoint02: {waypoints02[waypoints02.Length - 1].name}");
    }

    private bool HasReachedCurrentWaypoint(Transform[] waypoints, int index)
    {
        if (waypoints == null || index >= waypoints.Length)
            return false;

        Transform currentWaypoint = waypoints[index];
        if (currentWaypoint == null)
            return false;

        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
        bool closeEnough = distanceToWaypoint <= waypointReachDistance;
        bool agentReached = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        bool almostStopped = agent.velocity.magnitude < 0.1f;

        return closeEnough && (agentReached || almostStopped);
    }

    private void SetDestinationToCurrentWaypoint(Transform[] waypoints, int index)
    {
        if (waypoints != null && index < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[index];
            if (targetWaypoint != null)
            {
                agent.SetDestination(targetWaypoint.position);
                Debug.Log($"NPC indo para waypoint: {targetWaypoint.name}");
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

        bool isMovingNow = agent.velocity.magnitude > 0.1f;

        animator.SetBool("isWalking", isMovingNow && !isRunning);
        animator.SetBool("isRunning", isMovingNow && isRunning);
    }

    private void OnDrawGizmos()
    {
        DrawWaypointsGizmos(waypoints01, currentWaypointIndex, isMoving, Color.red);
        DrawWaypointsGizmos(waypoints02, currentWaypointIndex02, isMovingToWaypoints02, Color.cyan);
    }

    private void DrawWaypointsGizmos(Transform[] waypoints, int currentIndex, bool isMovingState, Color color)
    {
        if (waypoints == null || waypoints.Length == 0) return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.color = (i == currentIndex && isMovingState) ? Color.green : color;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
