using UnityEngine;
using UnityEngine.AI;

public class BossMovement : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossCharacter bossCharacter;
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent agent;

    [Header("Configuração de Movimento")]
    [SerializeField] private float minDistanceFromPlayer = 3f;
    [SerializeField] private float maxDistanceFromPlayer = 8f;
    [SerializeField] private float strafeDistance = 5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Velocidades")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float circleSpeed = 4f;

    [Header("Padrões de Movimento")]
    [SerializeField] private float circleMovementChance = 0.3f;
    [SerializeField] private float retreatChance = 0.2f;
    [SerializeField] private bool debugMode = false;
    
    private Vector3 targetPosition;
    private MovementPattern currentPattern = MovementPattern.Idle;
    private float patternTimer = 0f;
    private float patternDuration = 2f;
    private int circleDirection = 1;

    public bool canMove { get; set; } = true;
    public MovementPattern CurrentPattern => currentPattern;

    public enum MovementPattern
    {
        Idle,
        Approach,
        Retreat,
        Circle,
        Strafe
    }

    void Start()
    {
        if (bossCharacter == null) bossCharacter = GetComponent<BossCharacter>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = minDistanceFromPlayer;
        }
    }

    void Update()
    {
        if (!canMove || player == null || bossCharacter.isDead || agent == null)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (debugMode)
        {
            Debug.Log($"Boss - Distância: {distanceToPlayer:F2} | Padrão: {currentPattern} | CanMove: {canMove}");
        }
        
        UpdateMovementPattern(distanceToPlayer);
        ExecuteMovement(distanceToPlayer);
    }

    void UpdateMovementPattern(float distanceToPlayer)
    {
        patternTimer -= Time.deltaTime;

        if (patternTimer <= 0f || distanceToPlayer < minDistanceFromPlayer || distanceToPlayer > maxDistanceFromPlayer)
        {
            ChooseNewPattern(distanceToPlayer);
            patternTimer = patternDuration;
        }
    }

    void ChooseNewPattern(float distanceToPlayer)
    {
        if (distanceToPlayer > maxDistanceFromPlayer)
        {
            currentPattern = MovementPattern.Approach;
            agent.speed = chaseSpeed;
            patternDuration = Random.Range(2f, 4f);
            return;
        }

        if (distanceToPlayer < minDistanceFromPlayer)
        {
            currentPattern = MovementPattern.Retreat;
            agent.speed = patrolSpeed;
            patternDuration = Random.Range(1f, 2f);
            return;
        }

        float rand = Random.value;

        if (rand < circleMovementChance)
        {
            currentPattern = MovementPattern.Circle;
            circleDirection = Random.value > 0.5f ? 1 : -1;
            agent.speed = circleSpeed;
            patternDuration = Random.Range(2f, 4f);
        }
        else if (rand < circleMovementChance + retreatChance)
        {
            currentPattern = MovementPattern.Retreat;
            agent.speed = patrolSpeed;
            patternDuration = Random.Range(1f, 2f);
        }
        else if (rand < circleMovementChance + retreatChance + 0.2f)
        {
            currentPattern = MovementPattern.Strafe;
            circleDirection = Random.value > 0.5f ? 1 : -1;
            agent.speed = circleSpeed;
            patternDuration = Random.Range(1.5f, 3f);
        }
        else
        {
            currentPattern = MovementPattern.Idle;
            agent.speed = patrolSpeed;
            patternDuration = Random.Range(0.5f, 1.5f);
        }
    }

    void ExecuteMovement(float distanceToPlayer)
    {
        agent.isStopped = false;

        switch (currentPattern)
        {
            case MovementPattern.Approach:
                agent.SetDestination(player.position);
                if (debugMode) Debug.Log("Boss se aproximando do player");
                break;

            case MovementPattern.Retreat:
                Vector3 retreatDirection = (transform.position - player.position).normalized;
                Vector3 retreatPosition = transform.position + retreatDirection * 3f;
                agent.SetDestination(retreatPosition);
                if (debugMode) Debug.Log("Boss recuando");
                break;

            case MovementPattern.Circle:
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                Vector3 rightDirection = Vector3.Cross(Vector3.up, directionToPlayer);
                
                Vector3 circleOffset = rightDirection * circleDirection * strafeDistance;
                
                if (distanceToPlayer < strafeDistance - 1f)
                    circleOffset += directionToPlayer * -2f;
                else if (distanceToPlayer > strafeDistance + 1f)
                    circleOffset += directionToPlayer * 2f;
                
                Vector3 circlePosition = player.position + circleOffset;
                agent.SetDestination(circlePosition);
                if (debugMode) Debug.Log("Boss circulando");
                break;

            case MovementPattern.Strafe:
                Vector3 toPlayer = (player.position - transform.position).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, toPlayer);
                Vector3 strafePosition = transform.position + right * circleDirection * 3f;
                agent.SetDestination(strafePosition);
                if (debugMode) Debug.Log("Boss fazendo strafe");
                break;

            case MovementPattern.Idle:
                agent.isStopped = true;
                agent.ResetPath();
                if (debugMode) Debug.Log("Boss parado");
                break;
        }

        // Rotaciona suavemente em direção ao player
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetMovementPattern(MovementPattern pattern, float duration = 2f)
    {
        currentPattern = pattern;
        patternDuration = duration;
        patternTimer = duration;
    }

    public void StopMovement(float duration)
    {
        canMove = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        Invoke(nameof(ResumeMovement), duration);
    }

    void ResumeMovement()
    {
        canMove = true;
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    public void TeleportAroundPlayer(float distance)
    {
        if (player == null || agent == null) return;

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        randomDirection = randomDirection.normalized;

        Vector3 teleportPosition = player.position + randomDirection * distance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(teleportPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (debugMode) Debug.Log($"Boss teleportou para {hit.position}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, strafeDistance);
    }
}