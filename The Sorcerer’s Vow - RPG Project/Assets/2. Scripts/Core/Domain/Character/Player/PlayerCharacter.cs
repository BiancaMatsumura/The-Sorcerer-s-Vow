using System;
using System.Collections;
using _2._Scripts.Core.Domain.Character.Base;
using _2._Scripts.Core.Domain.Character.Player.Inputs;
using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;
using UnityEngine.UI;

namespace _2._Scripts.Core.Domain.Character.Player
{
    [Serializable]
    public class PlayerCharacter : BaseCharacter
    {

        public float maxEnergy;
        public float currentEnergy;
        public float energyReductionRate = 5f;
        public float energyRecoveryRate = 5f;

        [SerializeField] private Slider sliderLife;
        [SerializeField] private Slider sliderEnergy;


        [SerializeField]
        private InventoryController inventoryController;

        public override void Start()
        {
            base.Start();
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;


            currentEnergy = maxEnergy;
            sliderEnergy.maxValue = maxEnergy;
            sliderEnergy.value = currentEnergy;

            SaveLoadProgressManager.LoadAllSaveDataFromFile();
            inventoryController = GetComponent<InventoryController>();

        }

        public override void Update()
        {
            base.Update();
            sliderLife.value = currentHealth;
            sliderEnergy.value = currentEnergy;

            if (currentEnergy < maxEnergy)
            {
                bool isNotSprinting = true;

                // Checa se o personagem não está correndo
                var controller = GetComponent<ThirdPersonController>();
                if (controller != null)
                    isNotSprinting = !controller.IsSprinting();

                if (isNotSprinting)
                {
                    currentEnergy += energyRecoveryRate * Time.deltaTime;
                    if (currentEnergy > maxEnergy)
                        currentEnergy = maxEnergy;
                }
            }

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

        public void SetInitialPosition(Vector3 position)
        {
            CharacterController characterController = GetComponent<CharacterController>();
            characterController.Move(position - transform.position);
        }

        public void ReduceEnergy(float amount)
        {
            currentEnergy -= amount;
            if (currentEnergy < 0) currentEnergy = 0;
        }

        public void RecoverEnergy(float amount)
        {
            currentEnergy += amount;
            if (currentEnergy < 0) currentEnergy = 0;
        }

        public InventoryController InventoryController => inventoryController;
    }
}