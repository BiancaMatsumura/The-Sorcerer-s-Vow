using System.Collections;
using UnityEngine;
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
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 15f;
    //[SerializeField] private float attackRange = 5f; // opcional: distância mínima para atacar

    [Header("Configuração de Ações")]
    public float timeBetweenActions = 4f;
    private BossAIState currentState = BossAIState.Idle;
    private bool isChoosing = false;
    private float nextActionTime = 0f;

    void Start()
    {
        if (bossCharacter == null) bossCharacter = GetComponent<BossCharacter>();
        if (bossController == null) bossController = GetComponent<BossController>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (!player || currentState == BossAIState.Dead) return;

        float dist = Vector3.Distance(player.position, bossCharacter.transform.position);

        // Fora do range → fica parado
        if (dist > detectionRange)
        {
            bossController.SetState(BossState.Idle);
            currentState = BossAIState.Idle;
            return;
        }

        // Dentro do alcance, mas muito longe para atacar → boss "persegue"
        /*if (dist > attackRange)
        {
            bossController.SetState(BossState.Walk);
            bossCharacter.transform.position = Vector3.MoveTowards(
                bossCharacter.transform.position,
                player.position,
                Time.deltaTime * bossCharacter.Speed
            );
            return;
        }*/

        // Se está no alcance de ataque → escolhe ação
        if (Time.time >= nextActionTime && !isChoosing)
        {
            StartCoroutine(ChooseActionRoutine());
            nextActionTime = Time.time + timeBetweenActions;
        }
    }

    IEnumerator ChooseActionRoutine()
    {
        isChoosing = true;
        currentState = BossAIState.ChoosingAction;

        yield return new WaitForSeconds(1f); // "tempo de pensamento"

        int choice;

        // Primeira vida: só ataques básicos
        if (bossCharacter.currentLives == bossCharacter.totalLives)
        {
            choice = Random.Range(0, 2); // 0=normal, 1=área
        }
        else
        {
            // Segunda vida: todas as habilidades liberadas
            choice = Random.Range(0, 4); // 0=normal, 1=área, 2=totem, 3=vento
        }

        switch (choice)
        {
            case 0:
                bossController.SetState(BossState.AttackNormal);
                break;
            case 1:
                bossController.SetState(BossState.AttackArea);
                break;
            case 2:
                bossController.SetState(BossState.SpawnTotem);
                break;
            case 3:
                bossController.SetState(BossState.Wind);
                break;
        }

        currentState = BossAIState.Acting;

        yield return new WaitForSeconds(1f); // tempo mínimo de execução da ação

        currentState = BossAIState.Idle;
        isChoosing = false;
    }

    public void ForceDeath()
    {
        currentState = BossAIState.Dead;
        StopAllCoroutines();
        bossController.SetState(BossState.Idle);
        Debug.Log("Boss derrotado (forçado pela IA).");
    }
}
