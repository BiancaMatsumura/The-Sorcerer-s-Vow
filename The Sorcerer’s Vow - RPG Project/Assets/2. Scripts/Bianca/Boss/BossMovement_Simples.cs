using UnityEngine;
using UnityEngine.AI;

public class BossMovement_Simples : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;

    [Header("Configuração")]
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 8f;
    
    [Header("Velocidades")]
    [SerializeField] private float walkSpeed = 3f; // velocidade de caminhada normal
    [SerializeField] private float chaseSpeed = 5f; // velocidade ao perseguir de longe
    [SerializeField] private float retreatSpeed = 4f; // velocidade ao recuar

    public bool canMove = true;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = minDistance - 0.5f;
            agent.acceleration = 8f; // aceleração suave
        }
        else
        {
            Debug.LogError("NavMeshAgent não encontrado!");
        }

        if (player == null)
        {
            Debug.LogError("Player não encontrado! Verifique a tag 'Player'");
        }
    }

    void Update()
    {
        if (!canMove || player == null || agent == null)
        {
            animator.SetBool("isWalking", false);
            //Debug.LogWarning($"Boss parado - canMove: {canMove}, player: {player != null}, agent: {agent != null}");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        
        //Debug.Log($"<color=cyan>Boss - Distância do player: {distance:F2}</color>");

        // Se está muito longe, vai até o player (correndo)
        if (distance > maxDistance)
        {
            
            agent.isStopped = false;
            agent.speed = chaseSpeed; // mais rápido quando longe
            agent.SetDestination(player.position);
            animator.SetBool("isWalking", true);
            //Debug.Log($"<color=green>Boss PERSEGUINDO (longe) - Speed: {agent.speed}</color>");
        }
        // Se está muito perto, recua
        else if (distance < minDistance)
        {
            agent.isStopped = false;
            agent.speed = retreatSpeed; // velocidade média ao recuar
            Vector3 retreatDirection = (transform.position - player.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 3f;
            agent.SetDestination(retreatPosition);
            animator.SetBool("isWalking", true);
            //Debug.Log($"<color=yellow>Boss RECUANDO - Speed: {agent.speed}</color>");
        }
        // Se está na distância ideal, fica se movendo ao redor
        else
        {
            agent.isStopped = false;
            agent.speed = walkSpeed; // velocidade normal
            agent.SetDestination(player.position);
            animator.SetBool("isWalking", true);
            //Debug.Log($"<color=blue>Boss CIRCULANDO - Speed: {agent.speed}</color>");
        }

        // Sempre olha para o player
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                Quaternion.LookRotation(lookDirection), 
                5f * Time.deltaTime
            );
        }

        // Debug adicional
        //Debug.Log($"Agent - isStopped: {agent.isStopped}, hasPath: {agent.hasPath}, velocity: {agent.velocity.magnitude:F2}");
    }

    void OnDrawGizmos()
    {
        if (player == null) return;

        // Distância mínima (vermelho)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minDistance);

        // Distância máxima (azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, maxDistance);

        // Linha até o player
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, player.position);
    }

    // Método para parar movimento temporariamente (usado pela IA durante ataques)
    public void StopMovement(float duration)
    {

        canMove = false;
        animator.SetBool("isWalking", false);
        Invoke(nameof(ResumeMovement), duration);
    }

    void ResumeMovement()
    {
        canMove = true;
    }

    // Método de teleporte (usado na segunda fase)
    public void TeleportAroundPlayer(float distance)
    {
        if (player == null || agent == null) return;

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        randomDirection = randomDirection.normalized;

        Vector3 teleportPosition = player.position + randomDirection * distance;

        // Verifica se a posição está no NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(teleportPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }
}