using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using _2._Scripts.Core.Domain.Character.Base;

public enum BossAIState_Simple
{
    Idle,
    ChoosingAction,
    Acting,
    Dead
}

public class BossAI_Simples : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossCharacter bossCharacter;
    [SerializeField] private BossController bossController;
    [SerializeField] private BossMovement_Simples movement;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Configuração de Detecção")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 8f;

    [Header("Configuração de Ações")]
    public float timeBetweenActions = 4f;
    public float attackStopDuration = 2f;
    public bool debugMode = false;

    private BossAIState_Simple currentState = BossAIState_Simple.Idle;
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
        if (movement == null) movement = GetComponent<BossMovement_Simples>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (debugMode)
        {
            Debug.Log($"<color=magenta>BossAI iniciado - Character: {bossCharacter != null}, Controller: {bossController != null}, Movement: {movement != null}</color>");
        }
    }

    void Update()
    {
        if (player == null || currentState == BossAIState_Simple.Dead)
        {
            return;
        }

        // Verifica se o boss morreu
        if (bossCharacter != null && bossCharacter.isDead)
        {
            ForceDeath();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (debugMode)
        {
            Debug.Log($"<color=orange>BossAI - Distância: {distance:F2} | State: {currentState} | CanMove: {movement?.canMove}</color>");
        }

        // Fora do range de detecção → fica parado
        if (distance > detectionRange)
        {
            if (movement != null) movement.canMove = false;
            if (agent != null) agent.isStopped = true;
            if (bossController != null) bossController.SetState(BossState.Idle);
            currentState = BossAIState_Simple.Idle;

            if (debugMode) Debug.Log("<color=red>Boss DESATIVADO (fora do range)</color>");
            return;
        }

        // Dentro do range de detecção - ativa movimento
        if (movement != null && currentState == BossAIState_Simple.Idle)
        {
            movement.canMove = true;
            if (debugMode) Debug.Log("<color=green>Boss ATIVADO (dentro do range)</color>");
        }

        // Dentro do alcance de ataque → escolhe ação
        if (distance <= attackRange && Time.time >= nextActionTime && !isChoosing)
        {
            StartCoroutine(ChooseActionRoutine());
            nextActionTime = Time.time + timeBetweenActions;
        }
    }

    IEnumerator ChooseActionRoutine()
    {
        isChoosing = true;
        currentState = BossAIState_Simple.ChoosingAction;

        // Para o movimento enquanto decide
        if (movement != null)
        {
            movement.canMove = false;
        }
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        yield return new WaitForSeconds(0.5f);

        // Segunda fase: chance de teleportar antes de atacar
        if (bossCharacter != null && 
            bossCharacter.currentLives < bossCharacter.totalLives && 
            enableTeleportInSecondPhase && 
            Random.value < teleportChance)
        {
            TeleportAroundPlayer();
            yield return new WaitForSeconds(0.5f);
        }

        // Escolhe qual ataque fazer baseado na fase
        int choice;

        if (bossCharacter != null && bossCharacter.currentLives == bossCharacter.totalLives)
        {
            // Primeira vida: só ataques básicos (0=normal, 1=área)
            choice = Random.Range(0, 2);
        }
        else
        {
            // Segunda vida: todas as habilidades (0=normal, 1=área, 2=totem, 3=vento)
            choice = Random.Range(0, 4);
        }

        // Para completamente durante o ataque
        if (movement != null)
        {
            movement.canMove = false;
        }
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Executa o ataque escolhido
        ExecuteAttack(choice);

        currentState = BossAIState_Simple.Acting;

        // Aguarda a duração do ataque
        yield return new WaitForSeconds(attackStopDuration);

        // Retoma movimento após ataque
        if (movement != null)
        {
            movement.canMove = true;
        }
        if (agent != null)
        {
            agent.isStopped = false;
        }

        currentState = BossAIState_Simple.Idle;
        isChoosing = false;
    }

    void ExecuteAttack(int attackType)
    {
        if (bossController == null) return;

        switch (attackType)
        {
            case 0:
                bossController.SetState(BossState.AttackNormal);
                if (debugMode) Debug.Log("<color=yellow>Boss: Ataque Normal</color>");
                break;
            case 1:
                bossController.SetState(BossState.AttackArea);
                if (debugMode) Debug.Log("<color=yellow>Boss: Ataque em Área</color>");
                break;
            case 2:
                bossController.SetState(BossState.SpawnTotem);
                if (debugMode) Debug.Log("<color=yellow>Boss: Invocar Totem</color>");
                break;
            case 3:
                bossController.SetState(BossState.Wind);
                if (debugMode) Debug.Log("<color=yellow>Boss: Ataque de Vento</color>");
                break;
        }
    }

    void TeleportAroundPlayer()
    {
        if (player == null || agent == null) return;

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        randomDirection = randomDirection.normalized;

        Vector3 teleportPosition = player.position + randomDirection * teleportDistance;

        // Verifica se a posição está no NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(teleportPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            if (debugMode) Debug.Log($"<color=cyan>Boss teleportou para {hit.position}</color>");
        }
        else
        {
            if (debugMode) Debug.LogWarning("Não foi possível encontrar posição válida para teleporte");
        }
    }

    public void ForceDeath()
    {
        currentState = BossAIState_Simple.Dead;
        StopAllCoroutines();

        if (bossController != null)
        {
            bossController.SetState(BossState.Idle);
        }

        if (movement != null)
        {
            movement.canMove = false;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Debug.Log("<color=red>Boss derrotado!</color>");
    }

    // Métodos auxiliares para forçar comportamentos (opcional)
    public void ForceAttack(int attackType)
    {
        if (!isChoosing && currentState == BossAIState_Simple.Idle)
        {
            StartCoroutine(ForceAttackCoroutine(attackType));
        }
    }

    IEnumerator ForceAttackCoroutine(int attackType)
    {
        isChoosing = true;
        currentState = BossAIState_Simple.Acting;

        if (movement != null) movement.canMove = false;
        if (agent != null) agent.isStopped = true;

        ExecuteAttack(attackType);

        yield return new WaitForSeconds(attackStopDuration);

        if (movement != null) movement.canMove = true;
        if (agent != null) agent.isStopped = false;

        currentState = BossAIState_Simple.Idle;
        isChoosing = false;
    }
}