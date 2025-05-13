using System;
using System.Collections.Generic;
using _2._Scripts.Core.Domain.Character.Player;
using Inventory.Model;

namespace _2._Scripts.Core.Engine.Service.SaveLoad.Structure
{
    public class InventoryDTO
    {
        public Object inventoryItems;

        public InventoryDTO(PlayerCharacter playerCharacter)
        {
            inventoryItems = ScriptableSerializer.SerializeList<InventoryItem>(playerCharacter.InventoryController.InventoryData.inventoryItems);
        }
    }
}