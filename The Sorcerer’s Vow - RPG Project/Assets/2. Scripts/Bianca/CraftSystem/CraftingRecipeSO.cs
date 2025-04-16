using System.Collections.Generic;
using Inventory.Model;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    [System.Serializable]
    public struct RecipeItem
    {
        public ItemSO item;
        public int quantity;
    }
    public Frutos fruto;

    public List<RecipeItem> ingredients;
    public ItemSO resultItem;
    public int resultQuantity = 1;
    
}
