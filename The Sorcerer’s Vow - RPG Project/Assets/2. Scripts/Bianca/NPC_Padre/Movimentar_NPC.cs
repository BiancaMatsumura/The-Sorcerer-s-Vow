using System;
using UnityEngine;
using UnityEngine.AI;
using QuestSystem;

public class Movimentar_NPC : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] waypoints01;
    public Transform[] waypoints02;

    [Header("Missões que controlam movimento")]
    public SO_Quest questToStartMovement;     // Quando estiver completa, inicia movimento 1
    public SO_Quest questToStartMovement02;   // Quando estiver completa, inicia movimento 2

    [Header("Configurações de Movimento")]
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float runAfterSeconds = 5f;
    public float waypointReachDistance = 1f;

    [Header("Detecção do Player")]
    public Transform player;
    public float activationDistance = 5f;

    [Header("UI")]
    public GameObject interactionUI;
    private Transform mainCamera;
    public bool isActiveUI = false;

    private bool requirePlayerForMovement = false;


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

    private QuestManager questManager;

    private void Awake()
    {
        mainCamera = Camera.main?.transform;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ConfigureAgent();

        questManager = FindAnyObjectByType<QuestManager>();
        if (questManager == null)
            Debug.LogError("[Movimentar_NPC] ❌ QuestManager não encontrado!");
    }

    private bool IsPlayerClose()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= activationDistance;
    }

    private void Update()
    {
        // Mantém a UI virada para a câmera
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

    // ✅ Agora usa o QuestManager para checar se a missão foi completada
    private bool IsQuestCompleted(SO_Quest quest)
    {
        if (quest == null || questManager == null)
            return false;

        var activeQuest = questManager.GetQuestByID(quest.id);
        if (activeQuest == null)
            return false;

        return activeQuest.QuestStatus == QuestStatus.Completed;
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
        requirePlayerForMovement = true;

        if (interactionUI != null)
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
                StopMovement();
            else
                SetDestinationToCurrentWaypoint(waypoints01, currentWaypointIndex);
        }
    }

    private void StopMovement()
    {
        if (interactionUI != null)
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
        requirePlayerForMovement = false;

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
                StopMovement02();
            else
                SetDestinationToCurrentWaypoint(waypoints02, currentWaypointIndex02);
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
        // Se esse movimento exige player perto e o player não está perto -> pausa
        if (requirePlayerForMovement && !IsPlayerClose())
        {
            agent.isStopped = true;
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            return;
        }

        if (agent.isStopped)
            agent.isStopped = false;

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
