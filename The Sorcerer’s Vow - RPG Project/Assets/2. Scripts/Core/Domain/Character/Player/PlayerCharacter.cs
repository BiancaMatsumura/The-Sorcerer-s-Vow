using System;
using _2._Scripts.Core.Domain.Character.Base;
using _2._Scripts.Core.Domain.Character.Player.Inputs;
using _2._Scripts.Core.Engine.Service.SaveLoad;
using UnityEngine;

namespace _2._Scripts.Core.Domain.Character.Player
{
    [Serializable]
    public class PlayerCharacter : BaseCharacter
    {
        [SerializeField]
        private InventoryController inventoryController;

        [SerializeField] 
        private PlayerInputController playerInputController;
        
        public override void Start()
        {
            base.Start();
            SaveLoadProgressManager.LoadAllSaveDataFromFile();
            inventoryController = GetComponent<InventoryController>();
            playerInputController = GetComponent<PlayerInputController>();
        }

        public override void Update()
        {
            base.Update();
            // Debug.Log("PlayerCharacter Update");
        }
        
        public InventoryController InventoryController => inventoryController;
    }
}