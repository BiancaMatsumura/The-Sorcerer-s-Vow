using System.Collections.Generic;
using Inventory.Model;
using UnityEngine;

namespace ShopSystem
{
    [CreateAssetMenu(menuName = "Shop/ShopSO")]
    public class ShopSO : ScriptableObject
    {
        [SerializeField]
        private List<ShopItem> shopItems;

        public List<ShopItem> ShopItems => shopItems;
    }

    [System.Serializable]
    public struct ShopItem
    {
        public ItemSO item;
        public int price;
        public int minQuantity;
        public int maxQuantity;
    }

}
