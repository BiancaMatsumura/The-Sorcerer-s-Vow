using System;
using System.Collections;
using _2._Scripts.Core.Domain.Character.Base;
using UnityEngine;
using UnityEngine.UI;

namespace _2._Scripts.Core.Domain.Character.Player
{
    [Serializable]
    public class PlayerCharacter : BaseCharacter
    {
        [SerializeField] private Slider sliderLife;
       

        public override void Start()
        {
            base.Start();
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;

            Debug.Log("PlayerCharacter Start");
        }

        public override void Update()
        {
            base.Update();
            sliderLife.value = currentHealth;
            // Debug.Log("PlayerCharacter Update");
        }

        public void ChangeVelocityTemporarily(int newSpeed, float duration)
        {
            StopCoroutine(nameof(ResetSpeedCoroutine)); // previne sobreposição
            StartCoroutine(ResetSpeedCoroutine(newSpeed, duration));
        }

        private IEnumerator ResetSpeedCoroutine(int newSpeed, float duration)
        {
            int previousSpeed = Speed;
            ChangeVelocity(newSpeed);
            yield return new WaitForSeconds(duration);
            ChangeVelocity(previousSpeed);
        }

        
        
    }
}