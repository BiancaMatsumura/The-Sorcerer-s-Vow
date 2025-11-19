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

            // ✅ Corrige itens antigos já existentes (caso itemState esteja desatualizado)
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                var it = inventoryItems[i];
                if (!it.IsEmpty)
                {
                    it.itemState = SyncStateWithDefaults(it.item, it.itemState);
                    inventoryItems[i] = it;
                }
            }
        }

        // ============================================================
        //   🔥 SYNC ESTADO DO ITEM COM OS DEFAULTS (CORRIGIDO!)
        // ============================================================
        private List<ItemParameter> SyncStateWithDefaults(ItemSO item, List<ItemParameter> state)
        {
            var defaults = item.DefaultParametersList;

            // Item sem parâmetros
            if (defaults == null || defaults.Count == 0)
                return new List<ItemParameter>();

            // Se state for nulo ou vazio, cria uma CÓPIA PROFUNDA dos defaults
            if (state == null || state.Count == 0)
            {
                return CreateDeepCopyOfParameters(defaults);
            }

            // Se o tamanho for diferente, reconstrói com defaults
            if (state.Count != defaults.Count)
            {
                return CreateDeepCopyOfParameters(defaults);
            }

            // ✅ VERIFICAÇÃO CRÍTICA: Se algum itemParameter está nulo, reconstrói tudo
            for (int i = 0; i < state.Count; i++)
            {
                if (state[i].itemParameter == null)
                {
                    Debug.LogWarning($"⚠️ Detectado itemParameter nulo no item {item.Name}. Reconstruindo parâmetros.");
                    return CreateDeepCopyOfParameters(defaults);
                }
            }

            // Se está tudo certo, retorna cópia do state atual
            return new List<ItemParameter>(state);
        }

        // ✅ MÉTODO AUXILIAR: Cria cópia profunda garantindo que itemParameter seja copiado
        private List<ItemParameter> CreateDeepCopyOfParameters(List<ItemParameter> source)
        {
            if (source == null || source.Count == 0)
                return new List<ItemParameter>();

            List<ItemParameter> copy = new List<ItemParameter>();

            foreach (var param in source)
            {
                copy.Add(new ItemParameter
                {
                    itemParameter = param.itemParameter, // Copia a referência do ScriptableObject
                    value = param.value // Copia o valor
                });
            }

            return copy;
        }
        // ============================================================
        //                       ADICIONAR ITEM
        // ============================================================
        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if (item == null || quantity <= 0)
                return quantity;

            int initialQuantity = quantity;

            if (!item.IsStackable)
            {
                while (quantity > 0 && !IsInventoryFull())
                {
                    quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                }
            }
            else
            {
                quantity = AddStackableItem(item, quantity);
            }

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

                // ✅ SINCRONIZAÇÃO OBRIGATÓRIA
                itemState = SyncStateWithDefaults(item, itemState)
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
                        inventoryItems[i] = inventoryItems[i]
                            .ChangeQuantity(inventoryItems[i].quantity + amountToAdd);

                        quantity -= amountToAdd;

                        if (quantity <= 0)
                            return 0;
                    }
                }
            }

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

            // 🔥 Garante que seja compatível
            item.itemState = SyncStateWithDefaults(item.item, newState);

            inventoryItems[index] = item;
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        // ============================================================
        //                       REMOVER ITEM
        // ============================================================
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

        // ============================================================
        //                       OPERAÇÕES
        // ============================================================
        public void SwapItems(int indexA, int indexB)
        {
            if (indexA == indexB || indexA < 0 || indexB < 0
                || indexA >= inventoryItems.Count || indexB >= inventoryItems.Count)
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
    }

    // ============================================================
    //             STRUCT InventoryItem (sem alterações)
    // ============================================================
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
