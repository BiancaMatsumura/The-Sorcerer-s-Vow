using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private FloatValueSO currentHealth;

    [SerializeField] private Slider sliderLife;

    private void Start()
    {
        // Define a saúde inicial como o valor máximo
        currentHealth.Value = maxHealth;

        // Define os valores do slider de vida
        sliderLife.maxValue = maxHealth;
        sliderLife.value = currentHealth.Value;

        Debug.Log("Health initialized: " + currentHealth.Value);
    }

    public void Reduce(int damage)
    {
        // Reduz a saúde e atualiza o slider
        currentHealth.Value -= damage;
        if (currentHealth.Value < 0) currentHealth.Value = 0; // Previne que a saúde fique negativa
        sliderLife.value = currentHealth.Value;

        Debug.Log("Your Health is: " + currentHealth.Value);

        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    public void AddHealth(int healthBoost)
    {
        // Aumenta a saúde e atualiza o slider
        currentHealth.Value += healthBoost;
        if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth; // Previne que a saúde ultrapasse o máximo

        sliderLife.value = currentHealth.Value;

        Debug.Log("Your Health is: " + currentHealth.Value);
    }

    private void Die()
    {
        // Quando morrer, a saúde volta ao valor máximo (ou o valor inicial)
        Debug.Log("Died");
        currentHealth.Value = maxHealth; // Ou defina para 0 se preferir
        sliderLife.value = currentHealth.Value;
    }
}
