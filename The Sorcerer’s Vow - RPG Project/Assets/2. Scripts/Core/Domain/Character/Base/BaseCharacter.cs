using System;
using System.Collections;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Base
{
    [Serializable]
    public class BaseCharacter : MonoBehaviour
    {
        public string Name;
        public int Level;
        public int Speed;
        public int Strength;
        public float maxHealth;
        public float currentHealth;
        public float AttackPower;
        public float DefensePower;

        private float damageReductionMultiplier = 1f; // 1f = 100% do dano, 0.5f = 50%, etc.

        public virtual void Start()
        {
            currentHealth = maxHealth;
            Debug.Log("BaseCharacter Start");
        }

        public virtual void Update()
        {
            // Debug.Log("BaseCharacter Update");
        }

        public void ApplyDamageReductionTemporarily(float reductionPercentage, float duration)
        {
            float newMultiplier = 1f - (reductionPercentage / 100f);
            StopCoroutine("ResetDamageReduction"); 
            damageReductionMultiplier = Mathf.Clamp(newMultiplier, 0f, 1f);
            StartCoroutine(ResetDamageReductionAfterTime(duration));
        }

        private IEnumerator ResetDamageReductionAfterTime(float duration)
        {
            yield return new WaitForSeconds(duration);
            damageReductionMultiplier = 1f;
        }

        public void ReduceHealth(float damage)
        {
            int reducedDamage = Mathf.RoundToInt(damage * damageReductionMultiplier);
            currentHealth -= reducedDamage;
            if (currentHealth < 0) currentHealth = 0;

            Debug.Log($"Dano original: {damage}, Dano reduzido: {reducedDamage}, Vida atual: {currentHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void AddHealth(int healthBoost)
        {
            currentHealth += healthBoost;
            if (currentHealth > maxHealth) currentHealth = maxHealth;

            Debug.Log("Your Health is: " + currentHealth);
        }

        public virtual void Die()
        {
            Debug.Log("Died");
            currentHealth = maxHealth;
        }

        public void ChangeVelocity(int newSpeed)
        {
            Speed = newSpeed;

        }

        private float originalAttackPower;

        public void ChangeAttackPowerTemporarily(float newAttackPower, float duration)
        {
            originalAttackPower = AttackPower;
            AttackPower = newAttackPower;
            StartCoroutine(ResetAttackPowerAfterTime(duration));
        }
        IEnumerator ResetAttackPowerAfterTime(float duration)
        {
            yield return new WaitForSeconds(duration);
            AttackPower = originalAttackPower;
        }


        public void ChangeDefensePower(int newDefensePower)
        {
            DefensePower = newDefensePower;
        }
    }
}