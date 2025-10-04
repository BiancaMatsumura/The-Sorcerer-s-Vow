using System.Collections;
using UnityEngine;
using _2._Scripts.Core.Domain.Character.Base;
using UnityEngine.UI;

public class BossCharacter : BaseCharacter
{
    [Header("UI")]
    [SerializeField] private Slider sliderLife;
    private Transform mainCamera;

    [Header("Boss Configuração")]
    public int totalLives = 2;
    public int currentLives;

    [Header("Multiplicadores da Segunda Vida")]
    public float secondLifeAttackMultiplier = 1.5f;
    public float secondLifeDefenseMultiplier = 0.8f; // recebe menos dano
    public float secondLifeSpeedMultiplier = 1.2f;

    [Header("Referências")]
    [SerializeField] private BossController bossController; // controla ataques/animações
    //private Animator anim;

    public override void Start()
    {
        base.Start();
        mainCamera = Camera.main.transform;
        currentLives = totalLives;
        if (bossController == null) bossController = GetComponent<BossController>();

        if (sliderLife != null)
        {
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;
        }
    }

    public override void Update()
    {
        sliderLife.value = currentHealth;

        // Faz o slider olhar para a câmera
        if (sliderLife != null && mainCamera != null)
        {
            sliderLife.transform.LookAt(mainCamera);
        }
    }

    public override void ReduceHealth(float damage)
    {
        if (isDead) return;

        // Aplica defesa do boss (pode vir da BaseCharacter)
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


    public override void Die()
    {
        currentLives--;

        if (currentLives > 0)
        {
            Debug.Log($"Boss perdeu uma vida! Restam {currentLives}.");

            // "revive" com vida cheia
            currentHealth = maxHealth;

            // Buffs para a segunda vida
            AttackPower *= secondLifeAttackMultiplier;
            DefensePower *= secondLifeDefenseMultiplier;
            Speed = Mathf.RoundToInt(Speed * secondLifeSpeedMultiplier);

            // Ativa mais mecânicas na segunda vida
            if (bossController != null)
            {
                bossController.actionCooldown = 1.5f; // fica mais rápido
            }

            // Animação especial de transição de fase
            /*if (anim != null)
            {
                anim.SetTrigger("PhaseChange");
            }*/
        }
        else
        {
            Debug.Log("Boss derrotado de vez!");
            isDead = true;

            /* if (anim != null)
             {
                 anim.SetTrigger("Die");
             }*/

            // aqui você pode disparar cutscene, drop, vitória, etc.
        }
    }
}
