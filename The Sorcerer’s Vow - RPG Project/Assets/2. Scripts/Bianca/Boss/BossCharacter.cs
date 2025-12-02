using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using _2._Scripts.Core.Domain.Character.Base;
using UnityEngine.UI;

public class BossCharacter : BaseCharacter
{

    
    [Header("UI")]
    [SerializeField] private Slider sliderLife;
    private Transform mainCamera;

    [Header("Audios")]
    public AudioSource BossAudiosource;
    public AudioClip[] Sounds;

    [Header("Boss Configuração")]
    public int totalLives = 2;
    public int currentLives;

    [Header("Multiplicadores da Segunda Vida")]
    public float secondLifeAttackMultiplier = 1.5f;
    public float secondLifeDefenseMultiplier = 0.8f;
    public float secondLifeSpeedMultiplier = 1.2f;

    [Header("Referências")]
    [SerializeField] private BossController bossController;
    [SerializeField] private NavMeshAgent agent;
    private BossAI bossAI;

    public override void Start()
    {
        base.Start();
        mainCamera = Camera.main.transform;
        currentLives = totalLives;
        
        if (bossController == null) bossController = GetComponent<BossController>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (bossAI == null) bossAI = GetComponent<BossAI>();

        if (sliderLife != null)
        {
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;
        }

        // Configura NavMeshAgent
        if (agent != null)
        {
            agent.speed = Speed;
        }
    }

    public override void Update()
    {
        if (sliderLife != null)
        {
            sliderLife.value = currentHealth;

            // Faz o slider olhar para a câmera
            if (mainCamera != null)
            {
                sliderLife.transform.LookAt(mainCamera);
                sliderLife.transform.Rotate(0f, 180f, 0f);
            }
        }
    }

    public override void ReduceHealth(float damage)
    {
        if (isDead) return;

        AudioPlay(8);
        
        float finalDamage = Mathf.Max(damage - DefensePower, 1f);
        currentHealth -= finalDamage;
       
        if (sliderLife != null)
            sliderLife.value = currentHealth;

        Debug.Log($"Boss recebeu {finalDamage} de dano. Vida atual: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void AudioPlay(int Music)
    {
        BossAudiosource.resource = Sounds[Music];
        BossAudiosource.Play();


    }
    
   
    

    public override void Die()
    {
        currentLives--;

        if (currentLives > 0)
        {
            Debug.Log($"Boss perdeu uma vida! Restam {currentLives}. Entrando na FASE 2!");

            // "revive" com vida cheia
            currentHealth = maxHealth;

            // Buffs para a segunda vida
            AttackPower *= secondLifeAttackMultiplier;
            DefensePower *= secondLifeDefenseMultiplier;
            Speed = Mathf.RoundToInt(Speed * secondLifeSpeedMultiplier);

            // Atualiza velocidade do NavMeshAgent
            if (agent != null)
            {
                agent.speed = Speed;
            }

            // Ativa mais mecânicas na segunda vida
            if (bossController != null)
            {
                bossController.actionCooldown = 1.5f;
            }

            // Atualiza UI
            if (sliderLife != null)
            {
                sliderLife.maxValue = maxHealth;
                sliderLife.value = currentHealth;
            }

            Debug.Log($"Boss ficou mais forte! Attack: {AttackPower}, Defense: {DefensePower}, Speed: {Speed}");
        }
        else
        {
            Debug.Log("Boss derrotado de vez!");
            isDead = true;

            // Para movimento
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Notifica a IA
            if (bossAI != null)
            {
                bossAI.ForceDeath();
            }

            // Inicia coroutine de destruição
            StartCoroutine(DestroyAfterDeath());
        }
    }

    IEnumerator DestroyAfterDeath()
    {
        // Aqui você pode adicionar animação de morte
        // animator.Play("Die");
        
        yield return new WaitForSeconds(5f);
        
        // Aqui você pode disparar eventos de vitória, drops, etc.
        Debug.Log("Boss foi destruído!");
        
        Destroy(gameObject);
    }
}