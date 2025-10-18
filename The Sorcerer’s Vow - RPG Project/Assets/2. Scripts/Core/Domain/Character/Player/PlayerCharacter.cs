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
        private Animator _animator;

        [SerializeField] private Slider sliderLife;
        [SerializeField] private GameObject gameOverUI;


        [SerializeField]
        private InventoryController inventoryController;

        [Header("Fall Damage")]
        public float minFallHeight = 3f;    // altura mínima para começar a sofrer dano
        public float maxFallHeight = 10f;   // altura máxima para dano máximo
        public float maxFallDamage = 100f;  // dano máximo de queda

        private bool isFalling = false;
        private float fallStartY;
        private CharacterController _controller;

        public override void Start()
        {
            base.Start();
            sliderLife.maxValue = maxHealth;
            sliderLife.value = currentHealth;

            _animator = GetComponent<Animator>();

            SaveLoadProgressManager.LoadAllSaveDataFromFile();
            inventoryController = GetComponent<InventoryController>();
            _controller = GetComponent<CharacterController>();


        }

        public override void Update()
        {
            base.Update();
            sliderLife.value = currentHealth;
            HandleFallDamage();


            if (isDead)
            {
                _animator.Play("DIeAnimation");
                gameOverUI.SetActive(true);

            }


        }

        private void HandleFallDamage()
        {
            if (_controller.isGrounded)
            {
                if (isFalling)
                {
                    isFalling = false;
                    float fallDistance = fallStartY - transform.position.y;
                    if (fallDistance > minFallHeight)
                    {
                        ApplyFallDamage(fallDistance);
                    }
                }
            }
            else
            {
                if (!isFalling)
                {
                    isFalling = true;
                    fallStartY = transform.position.y;
                }
            }
        }

        private void ApplyFallDamage(float fallDistance)
        {
            float damage = Mathf.Lerp(0, maxFallDamage, (fallDistance - minFallHeight) / (maxFallHeight - minFallHeight));
            damage = Mathf.Clamp(damage, 0, maxFallDamage);

            ReduceHealth(damage);
            Debug.Log($"Queda de {fallDistance:F1} metros! Dano aplicado: {damage:F1}");
        }


        public override void ReduceHealth(float damage)
        {
            base.ReduceHealth(damage);
            _animator.Play("HitAnimation");
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