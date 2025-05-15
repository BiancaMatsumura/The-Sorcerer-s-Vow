using System;
using System.Collections;
using _2._Scripts.Core.Domain.Character.Base;
using _2._Scripts.Core.Domain.Character.Player.Inputs;
using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace _2._Scripts.Core.Domain.Character.Player
{
    [Serializable]
    public class PlayerCharacter : BaseCharacter
    {
        
        [SerializeField] private Slider sliderLife;
       

        [SerializeField]
        private InventoryController inventoryController;

        [SerializeField] 
        private PlayerInputController playerInputController;
        
        public Scene currentScene;

        
        public override void Start()
        {
            base.Start();
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;

            Debug.Log("PlayerCharacter Start");
            SaveLoadProgressManager.LoadAllSaveDataFromFile();
            inventoryController = GetComponent<InventoryController>();
            playerInputController = GetComponent<PlayerInputController>();
            currentScene = SceneManager.GetActiveScene();
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

        public void SetInitialPosition(Vector3 position)
        {
            CharacterController characterController = GetComponent<CharacterController>();
            characterController.Move(position - transform.position);
        }
        
        
        public InventoryController InventoryController => inventoryController;
    }
}