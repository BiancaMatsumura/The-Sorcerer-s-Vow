using _2._Scripts.Core.Domain.Character.Base;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
using QuestSystem;

public enum EnemyState
{
    Patrol,
    Chase,
    Battle
}

public class Enemy : BaseCharacter
{
    public int enemyID;

    [Header("UI")]
    [SerializeField] private Slider sliderLife;
    private Transform mainCamera;

    [Header("AI Settings")]
    public EnemyState currentState;
    public Transform[] patrolPoints;
    public float chaseRange = 10f;
    public float attackRange = 2f;
    public Transform target;

    private int currentPatrolIndex;
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Attack Settings")]
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;


    public Collider weaponCollider;

    private bool IsDead = false;

    public void EnableCollider()
    {
        weaponCollider.enabled = true;
    }

    public void DisableCollider()
    {
        weaponCollider.enabled = false;
    }

    public override void Start()
    {

        mainCamera = Camera.main.transform;

        base.Start();

        // Se não tiver pontos setados manualmente, busca no manager
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            PatrolManager manager = FindFirstObjectByType<PatrolManager>();
            if (manager != null)
            {
                patrolPoints = manager.GetPatrolPoints();
            }
        }

        sliderLife.maxValue = maxHealth;
        sliderLife.value = currentHealth;

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogWarning($"{gameObject.name} não possui Animator nem em filhos!");
            else if (animator.runtimeAnimatorController == null)
                Debug.LogWarning($"{gameObject.name} Animator (em filho) sem Controller atribuído!");
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{gameObject.name} Animator sem Controller atribuído!");
        }

        agent = GetComponent<NavMeshAgent>();



        currentState = EnemyState.Patrol;

        if (patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }

    public override void Update()
    {
        base.Update();

        sliderLife.value = currentHealth;

        if (sliderLife.gameObject.activeSelf && target != null)
        {
            sliderLife.transform.LookAt(mainCamera);
            sliderLife.transform.Rotate(0f, 180f, 0f);
        }

        if (!IsDead)
        {
            UpdateState();

            switch (currentState)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    Chase();
                    break;
                case EnemyState.Battle:
                    Battle();
                    break;
            }

            UpdateAnimator();
        }


    }
    public override void ReduceHealth(float damage)
    {
        if (IsDead) return; // já está morto, ignora novo dano

        if (!IsDead)
        {
            animator.Play("HitAnimation");
        }

        base.ReduceHealth(damage);
    }
    private void UpdateState()
    {
        if (target == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            agent.stoppingDistance = attackRange - 0.1f; // ou 1f se quiser um pouco mais longe

            agent.isStopped = true; // Para o agente quando em batalha
            currentState = EnemyState.Battle;
        }
        else if (distance <= chaseRange)
        {
            currentState = EnemyState.Chase;
            animator.SetBool("IsAttacking", false);
        }
        else
        {
            currentState = EnemyState.Patrol;
            animator.SetBool("IsAttacking", false);
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                agent.speed = 2f; // velocidade de caminhada
                break;
            case EnemyState.Chase:
                agent.speed = 5f; // velocidade de corrida
                break;
            case EnemyState.Battle:
                agent.stoppingDistance = attackRange - 0.1f; // ou 1f se quiser um pouco mais longe

                agent.speed = 0f;
                agent.isStopped = true;
                break;
        }

    }



    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }
    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;

        if (patrolPoints.Length > 0 && agent != null)
            GoToNextPatrolPoint();
    }


    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        int randomIndex = Random.Range(0, patrolPoints.Length);

        // Evita repetir o mesmo ponto se possível
        while (randomIndex == currentPatrolIndex && patrolPoints.Length > 1)
        {
            randomIndex = Random.Range(0, patrolPoints.Length);
        }

        currentPatrolIndex = randomIndex;
        agent.destination = patrolPoints[currentPatrolIndex].position;
    }


    private void Chase()
    {
        if (target == null) return;

        agent.isStopped = false;
        agent.destination = target.position;
    }

    private void Battle()
    {
        if (target == null) return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.stoppingDistance = attackRange - 1f; // ou 1f se quiser um pouco mais longe

        transform.LookAt(target);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack(); // Aqui que acontece a animação
            lastAttackTime = Time.time;
        }
    }


    private void Attack()
    {

        animator.Play("Attack");

    }


    private void UpdateAnimator()
    {
        if (animator == null || agent == null) return;

        // velocidade atual (m/s)
        float currentVelocity = agent.velocity.magnitude;

        // aplica no Animator com damping (suaviza a transição)
        animator.SetFloat("Speed", currentVelocity, 0.1f, Time.deltaTime);

        animator.SetBool("IsAttacking", currentState == EnemyState.Battle);
    }





    public override void Die()
    {
        if (IsDead)
            return;
        IsDead = true;
        agent.enabled = false;
        animator.Play("Die");
        currentHealth = 0;
        StartCoroutine(DestroyAfterDeath());
        Debug.Log($"QuestEvents: Triggering EnemyKilled for enemyID {enemyID}");
        QuestEvents.TriggerEnemyKilled(enemyID);

    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }
}
