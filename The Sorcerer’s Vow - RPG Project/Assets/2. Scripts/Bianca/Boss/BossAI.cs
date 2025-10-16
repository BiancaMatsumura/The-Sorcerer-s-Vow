using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using _2._Scripts.Core.Domain.Character.Base;

public enum BossAIState
{
    Idle,
    ChoosingAction,
    Acting,
    Dead
}

public class BossAI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossCharacter bossCharacter;
    [SerializeField] private BossController bossController;
    [SerializeField] private BossMovement bossMovement;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    
    [Header("Configuração de Detecção")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 8f;

    [Header("Configuração de Ações")]
    public float timeBetweenActions = 4f;
    public float attackStopDuration = 2f;
    public bool debugMode = false;
    
    private BossAIState currentState = BossAIState.Idle;
    private bool isChoosing = false;
    private float nextActionTime = 0f;

    [Header("Segunda Fase - Mecânicas Especiais")]
    public bool enableTeleportInSecondPhase = true;
    public float teleportChance = 0.15f;
    public float teleportDistance = 10f;

    void Start()
    {
        if (bossCharacter == null) bossCharacter = GetComponent<BossCharacter>();
        if (bossController == null) bossController = GetComponent<BossController>();
        if (bossMovement == null) bossMovement = GetComponent<BossMovement>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player || currentState == BossAIState.Dead || bossCharacter.isDead) return;

        float dist = Vector3.Distance(player.position, bossCharacter.transform.position);

        if (debugMode)
        {
            Debug.Log($"BossAI - Distância: {dist:F2} | State: {currentState} | CanMove: {bossMovement?.canMove}");
        }

        // Fora do range → fica parado
        if (dist > detectionRange)
        {
            bossController.SetState(BossState.Idle);
            currentState = BossAIState.Idle;
            if (bossMovement != null) bossMovement.canMove = false;
            if (agent != null) agent.isStopped = true;
            
            if (debugMode) Debug.Log("Boss fora do range de detecção");
            return;
        }

        // Dentro do alcance de detecção
        if (bossMovement != null)
        {
            bossMovement.canMove = true;
        }

        // Dentro do alcance de ataque → escolhe ação
        if (dist <= attackRange && Time.time >= nextActionTime && !isChoosing)
        {
            StartCoroutine(ChooseActionRoutine());
            nextActionTime = Time.time + timeBetweenActions;
        }
    }

    IEnumerator ChooseActionRoutine()
    {
        isChoosing = true;
        currentState = BossAIState.ChoosingAction;

        // Para o movimento enquanto decide
        if (bossMovement != null)
        {
            bossMovement.canMove = false;
        }
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        yield return new WaitForSeconds(0.5f);

        // Segunda fase: chance de teleportar
        if (bossCharacter.currentLives < bossCharacter.totalLives && 
            enableTeleportInSecondPhase && 
            Random.value < teleportChance &&
            bossMovement != null)
        {
            Debug.Log("Boss teleportou!");
            bossMovement.TeleportAroundPlayer(teleportDistance);
            yield return new WaitForSeconds(0.5f);
        }

        int choice;

        // Primeira vida: só ataques básicos
        if (bossCharacter.currentLives == bossCharacter.totalLives)
        {
            choice = Random.Range(0, 2);
        }
        else
        {
            // Segunda vida: todas as habilidades
            choice = Random.Range(0, 4);
        }

        // Para completamente durante o ataque
        if (bossMovement != null)
        {
            bossMovement.StopMovement(attackStopDuration);
        }
        if (agent != null)
        {
            agent.isStopped = true;
        }

        switch (choice)
        {
            case 0:
                bossController.SetState(BossState.AttackNormal);
                if (debugMode) Debug.Log("Boss: Ataque Normal");
                break;
            case 1:
                bossController.SetState(BossState.AttackArea);
                if (debugMode) Debug.Log("Boss: Ataque em Área");
                break;
            case 2:
                bossController.SetState(BossState.SpawnTotem);
                if (debugMode) Debug.Log("Boss: Invocar Totem");
                break;
            case 3:
                bossController.SetState(BossState.Wind);
                if (debugMode) Debug.Log("Boss: Ataque de Vento");
                break;
        }

        currentState = BossAIState.Acting;

        yield return new WaitForSeconds(attackStopDuration);

        // Retoma movimento
        if (bossMovement != null)
        {
            bossMovement.canMove = true;
        }
        if (agent != null)
        {
            agent.isStopped = false;
        }

        currentState = BossAIState.Idle;
        isChoosing = false;
    }

    public void ForceDeath()
    {
        currentState = BossAIState.Dead;
        StopAllCoroutines();
        bossController.SetState(BossState.Idle);
        
        if (bossMovement != null) bossMovement.canMove = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        Debug.Log("Boss derrotado (forçado pela IA).");
    }

    public void ForceAggressiveMovement(float duration)
    {
        if (bossMovement != null)
        {
            bossMovement.SetMovementPattern(BossMovement.MovementPattern.Approach, duration);
        }
    }

    public void ForceRetreat(float duration)
    {
        if (bossMovement != null)
        {
            bossMovement.SetMovementPattern(BossMovement.MovementPattern.Retreat, duration);
        }
    }
}