using UnityEngine;
using Inventory.Model;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    [SerializeField] private InventorySO inventory;

    public bool CanCraft(CraftingRecipeSO recipe)
    {
        var inventoryState = inventory.GetCurrentInventoryState();

        foreach (var ingredient in recipe.ingredients)
        {
            int totalFound = 0;
            foreach (var item in inventoryState.Values)
            {
                if (item.item.ID == ingredient.item.ID)
                    totalFound += item.quantity;
            }

            if (totalFound < ingredient.quantity)
                return false;
        }

        return true;
    }

    public void Craft(CraftingRecipeSO recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.Log("Faltam ingredientes.");
            return;
        }

        // Remover ingredientes
        foreach (var ingredient in recipe.ingredients)
        {
            int remaining = ingredient.quantity;
            for (int i = 0; i < inventory.Size && remaining > 0; i++)
            {
                var slot = inventory.GetItemAt(i);
                if (slot.IsEmpty || slot.item.ID != ingredient.item.ID)
                    continue;

                int removeAmount = Mathf.Min(slot.quantity, remaining);
                inventory.RemoveItem(i, removeAmount);
                remaining -= removeAmount;
            }
        }

        // Adicionar item final
        inventory.AddItem(recipe.resultItem, recipe.resultQuantity);
        Debug.Log($"Crafted {recipe.resultQuantity}x {recipe.resultItem.Name}");
    }
}
