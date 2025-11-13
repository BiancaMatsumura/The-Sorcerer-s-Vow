using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuestSystem;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Inventory/InventorySO")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField] public List<InventoryItem> inventoryItems;
        [field: SerializeField] public int Size { get; private set; } = 50;

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

        public void Initialize()
        {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++)
                inventoryItems.Add(InventoryItem.GetEmptyItem());
        }

        #region === ADICIONAR ITEM ===
        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if (item == null || quantity <= 0)
                return quantity;

            int initialQuantity = quantity;

            // Itens não empilháveis: adiciona 1 por slot
            if (!item.IsStackable)
            {
                while (quantity > 0 && !IsInventoryFull())
                {
                    quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                }
            }
            else
            {
                // Itens empilháveis: tenta adicionar no mesmo stack ou cria novos
                quantity = AddStackableItem(item, quantity);
            }

            // ✅ Atualiza apenas se algo foi adicionado
            if (quantity < initialQuantity)
            {
                QuestEvents.TriggerItemCollected(item.ID);
                InformAboutChange();
            }

            return quantity;
        }

        public void AddItem(InventoryItem inventoryItem)
        {
            AddItem(inventoryItem.item, inventoryItem.quantity, inventoryItem.itemState);
        }


        private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            var newItem = new InventoryItem
            {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState ?? item.DefaultParametersList)
            };

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }

            return 0;
        }

        private int AddStackableItem(ItemSO item, int quantity)
        {
            // 🔹 Primeiro tenta preencher stacks existentes
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;

                if (inventoryItems[i].item.ID == item.ID)
                {
                    int space = item.MaxStackSize - inventoryItems[i].quantity;
                    int amountToAdd = Mathf.Min(quantity, space);

                    if (amountToAdd > 0)
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].quantity + amountToAdd);
                        quantity -= amountToAdd;

                        if (quantity <= 0)
                            return 0;
                    }
                }
            }

            // 🔹 Se ainda sobrou, cria novos stacks
            while (quantity > 0 && !IsInventoryFull())
            {
                int amountToAdd = Mathf.Min(quantity, item.MaxStackSize);
                AddItemToFirstFreeSlot(item, amountToAdd);
                quantity -= amountToAdd;
            }

            return quantity;
        }

        public bool InventoryIsFull()
        {
            foreach (var item in inventoryItems)
            {
                if (item.IsEmpty)
                    return false;
            }
            return true;
        }

        public void SetItemState(int index, List<ItemParameter> newState)
        {
            if (index < 0 || index >= inventoryItems.Count)
                return;

            InventoryItem item = inventoryItems[index];
            if (item.IsEmpty)
                return;

            item.itemState = newState;
            inventoryItems[index] = item;
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        #endregion

        #region === REMOVER ITEM ===
        public void RemoveItem(int index, int amount)
        {
            if (index < 0 || index >= inventoryItems.Count)
                return;

            if (inventoryItems[index].IsEmpty)
                return;

            int remainder = inventoryItems[index].quantity - amount;

            if (remainder <= 0)
                inventoryItems[index] = InventoryItem.GetEmptyItem();
            else
                inventoryItems[index] = inventoryItems[index].ChangeQuantity(remainder);

            InformAboutChange();
        }
        #endregion

        #region === OPERAÇÕES GERAIS ===
        public void SwapItems(int indexA, int indexB)
        {
            if (indexA == indexB || indexA < 0 || indexB < 0 || indexA >= inventoryItems.Count || indexB >= inventoryItems.Count)
                return;

            (inventoryItems[indexA], inventoryItems[indexB]) = (inventoryItems[indexB], inventoryItems[indexA]);
            InformAboutChange();
        }

        public bool HasItem(ItemSO requiredItem)
        {
            return inventoryItems.Any(slot =>
                !slot.IsEmpty &&
                slot.item == requiredItem &&
                slot.quantity > 0);
        }

        public bool CanAddItem(ItemSO item, int quantity)
        {
            if (!item.IsStackable)
            {
                int freeSlots = inventoryItems.Count(i => i.IsEmpty);
                return freeSlots >= quantity;
            }
            else
            {
                foreach (var invItem in inventoryItems)
                {
                    if (invItem.IsEmpty)
                        return true;

                    if (invItem.item == item && invItem.quantity < item.MaxStackSize)
                        return true;
                }
                return false;
            }
        }

        private bool IsInventoryFull() =>
            !inventoryItems.Any(i => i.IsEmpty);

        private void InformAboutChange() =>
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        #endregion

        #region === ESTADO DO INVENTÁRIO ===
        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            var dict = new Dictionary<int, InventoryItem>();
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (!inventoryItems[i].IsEmpty)
                    dict[i] = inventoryItems[i];
            }
            return dict;
        }

        public InventoryItem GetItemAt(int index)
        {
            if (index < 0 || index >= inventoryItems.Count)
                return InventoryItem.GetEmptyItem();
            return inventoryItems[index];
        }
        #endregion
    }

    [Serializable]
    public struct InventoryItem
    {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;
        public bool IsEmpty => item == null;

        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(itemState)
            };
        }

        public static InventoryItem GetEmptyItem() => new InventoryItem
        {
            item = null,
            quantity = 0,
            itemState = new List<ItemParameter>()
        };
    }
}
