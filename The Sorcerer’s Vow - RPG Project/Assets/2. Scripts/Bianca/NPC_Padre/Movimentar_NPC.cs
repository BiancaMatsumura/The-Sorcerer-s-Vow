using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Movimentar_NPC : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints01;
    public Transform[] waypoints02;

    [Header("Missões que controlam movimento")]
    public Quest_ questToStartMovement;     // Quando esta estiver completa, inicia movimento 1
    public Quest_ questToStartMovement02;   // Quando esta estiver completa, inicia movimento 2


    private enum QuestState
    {
        NotStarted,
        InProgress,
        Completed
    }
    private QuestState questState = QuestState.NotStarted;

    [Header("Configurações de Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float runAfterSeconds = 5f;
    public float waypointReachDistance = 1f;

    [Header("Detecção do Player")]
    public Transform player; // arraste o player na Unity
    public float activationDistance = 5f; // distância mínima para NPC começar a se mover

    [Header("UI")]
    public GameObject interactionUI;
    private Transform mainCamera;
    public bool isActiveUI = false;

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
        mainCamera = Camera.main.transform;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();
    }

    private bool IsPlayerClose()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= activationDistance;
    }


    private void Update()
    {
        if (mainCamera != null && interactionUI != null)
        {
            interactionUI.transform.LookAt(mainCamera);
            interactionUI.transform.Rotate(0f, 180f, 0f);
        }

        // MOVIMENTO 1
        if (!isMoving && !hasStartedMovement && questToStartMovement != null && IsQuestCompleted(questToStartMovement) && IsPlayerClose())
        {
            StartMovement();
            hasStartedMovement = true;
        }

        if (isMoving)
            UpdateMovement();

        // MOVIMENTO 2
        if (!isMovingToWaypoints02 && !hasStartedMovement02 && questToStartMovement02 != null && IsQuestCompleted(questToStartMovement02) && IsPlayerClose())
        {
            StartMovement02();
            hasStartedMovement02 = true;
        }

        if (isMovingToWaypoints02)
            UpdateMovement02();
    }


    private bool IsQuestCompleted(Quest_ quest)
    {
        if (quest == null || QuestController.Instance == null)
            return false;

        var progress = QuestController.Instance.activeQuests.Find(q => q.quest.questID == quest.questID);
        return progress != null && progress.IsQuestCompleted;
    }

    private bool IsQuestActive(Quest_ quest)
    {
        if (quest == null || QuestController.Instance == null)
            return false;

        return QuestController.Instance.IsQuestActive(quest.questID);
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
        interactionUI.SetActive(true);
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
        interactionUI.SetActive(false);

        isMoving = false;
        isRunning = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);

        Debug.Log($"NPC parou no último waypoint01: {waypoints01[waypoints01.Length - 1].name}");

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
        // Se o player estiver longe, PAUSA o agente
        if (!IsPlayerClose())
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            return;
        }

        // Se o player voltou perto, RETOMA o movimento
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        // Verifica se deve correr
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
