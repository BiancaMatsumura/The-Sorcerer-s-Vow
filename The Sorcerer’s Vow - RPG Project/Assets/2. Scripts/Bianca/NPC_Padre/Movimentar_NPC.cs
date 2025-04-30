using System;
using UnityEngine;

public class Movimentar_NPC : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public float rotationSpeed = 5f;
    public float waypointReachedThreshold = 0.5f;
    public float groundCheckDistance = 0.3f;  // Distância para verificar o chão
    public LayerMask groundLayer;  // Layer do terreno

    public bool isMoving = false;
    private int currentWaypointIndex = 0;

    public QuestData questTrigger;
    Animator animator;

    Rigidbody rb;
    private BoxCollider boxCollider;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        
        // Configurações do Rigidbody para terrenos irregulares
        if (rb != null)
        {
            rb.useGravity = true;  // Mantenha a gravidade ativa
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;  // Permita rotação apenas no eixo Y
            rb.interpolation = RigidbodyInterpolation.Interpolate;  // Suavize o movimento
        }
    }

    void Start()
    {
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            SnapToGround();  // Coloca o NPC no chão ao iniciar
        }
        
        // Se a layer do terreno não for definida, use a layer "Default"
        if (groundLayer == 0)
            groundLayer = 1 << LayerMask.NameToLayer("Default");
    }

    void Update()
    {
        if (questTrigger != null && questTrigger.isCompleted)
        {
            isMoving = true;
            animator.SetBool("isWalking", true);
            boxCollider.isTrigger = false;
        }

        if (waypoints.Length == 0) return;

        if (isMoving)
        {
            boxCollider.isTrigger = false;
            animator.SetBool("isWalking", true);
            
            // Calcular direção apenas para rotação (o movimento será feito no FixedUpdate)
            if (currentWaypointIndex < waypoints.Length)
            {
                // Projeta o waypoint na mesma altura do personagem para rotação mais natural
                Vector3 targetPos = waypoints[currentWaypointIndex].position;
                Vector3 horizontalTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
                Vector3 direction = (horizontalTarget - transform.position).normalized;
                
                // Rotaciona apenas se estiver se movendo
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
            boxCollider.isTrigger = true;
            rb.linearVelocity = Vector3.zero;
        }
    }

    [Obsolete]
    private void FixedUpdate()
    {
        if (isMoving && waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
        {
            MoveToWaypoint();
        }
    }

    [Obsolete]
    private void MoveToWaypoint()
    {
        if (!isMoving || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        
        // Cria um vetor direção apenas no plano XZ (horizontal)
        Vector3 targetPos = targetWaypoint.position;
        Vector3 currentPos = transform.position;
        Vector3 horizontalDirection = new Vector3(targetPos.x - currentPos.x, 0, targetPos.z - currentPos.z).normalized;
        
        // Aplica a velocidade apenas nos eixos X e Z para permitir que a gravidade atue no Y
        Vector3 targetVelocity = horizontalDirection * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        
        // Verifica se chegou ao waypoint (apenas distância horizontal)
        float horizontalDistance = Vector2.Distance(
            new Vector2(currentPos.x, currentPos.z), 
            new Vector2(targetPos.x, targetPos.z)
        );
        
        if (horizontalDistance < waypointReachedThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                isMoving = false;
                animator.SetBool("isWalking", false);
                currentWaypointIndex = waypoints.Length - 1;
                boxCollider.enabled = true;
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
    
    // Função para garantir que o NPC esteja no chão
    private void SnapToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 10f, groundLayer))
        {
            transform.position = hit.point;
        }
    }
    
    // Desenha os waypoints e o caminho entre eles
    void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(waypoints[i].position, waypointReachedThreshold);
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);

                    if (i < waypoints.Length - 1 && waypoints[i+1] != null)
                    {
                        Gizmos.color = Color.yellow;
                        // Desenha uma linha entre waypoints com pequenos segmentos para visualizar melhor
                        Vector3 start = waypoints[i].position;
                        Vector3 end = waypoints[i+1].position;
                        int segments = 10;
                        
                        for (int j = 0; j < segments; j++)
                        {
                            float t1 = j / (float)segments;
                            float t2 = (j + 1) / (float)segments;
                            Vector3 pos1 = Vector3.Lerp(start, end, t1);
                            Vector3 pos2 = Vector3.Lerp(start, end, t2);
                            
                            // Opcional: projeta a linha no terreno
                            RaycastHit hit1, hit2;
                            if (Physics.Raycast(pos1 + Vector3.up * 5, Vector3.down, out hit1, 10f, groundLayer))
                                pos1 = hit1.point + Vector3.up * 0.1f;
                                
                            if (Physics.Raycast(pos2 + Vector3.up * 5, Vector3.down, out hit2, 10f, groundLayer))
                                pos2 = hit2.point + Vector3.up * 0.1f;
                                
                            Gizmos.DrawLine(pos1, pos2);
                        }
                    }
                }
            }
        }
    }
    
    // Desenha raios de verificação do terreno (visíveis apenas durante gameplay)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}