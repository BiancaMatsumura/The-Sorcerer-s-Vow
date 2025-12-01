using Inventory.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShopSystem
{
    public class Shop : MonoBehaviour
    {
        [SerializeField]
        public ShopSO ShopSO;
        
        public AudioSource AudioS;
        public AudioClip[] AudioSounds;
        [SerializeField]
        private InventorySO playerInventory;
        [SerializeField]
        private ItemSO currencyItem; // <- Aqui você arrasta o seu "Moeda" ItemSO no inspector

        private HashSet<int> purchasedItemIndices = new HashSet<int>();



        public void BuyItem(int shopItemIndex, int quantity)
        {
            if (shopItemIndex < 0 || shopItemIndex >= ShopSO.ShopItems.Count)
                return;

            ShopItem itemToBuy = ShopSO.ShopItems[shopItemIndex];
            int playerMoney = GetCurrencyAmount();
            int totalPrice = itemToBuy.price * quantity;

            if (playerMoney >= totalPrice)
            {
                int remainingQuantity = playerInventory.AddItem(itemToBuy.item, quantity);
                int purchasedQuantity = quantity - remainingQuantity;

                if (purchasedQuantity > 0)
                {
                    AudioPlay(1);
                    RemoveCurrency(itemToBuy.price * purchasedQuantity);
                    Debug.Log($"Comprou {purchasedQuantity}x {itemToBuy.item.name}");


                }
                else
                {
                    Debug.Log("Inventário cheio, não foi possível comprar o item.");
                }
            }
            else
            {
                AudioPlay(0);
                Debug.Log("Dinheiro insuficiente para comprar este item.");
            }
        }


        public bool HasItemBeenPurchased(int index)
        {
            return purchasedItemIndices.Contains(index);
        }



        public void SellItem(int inventoryIndex)
        {
            InventoryItem itemToSell = playerInventory.GetItemAt(inventoryIndex);
            if (itemToSell.IsEmpty)
                return;

            int sellPrice = GetSellPrice(itemToSell.item);

            playerInventory.RemoveItem(inventoryIndex, 1);
            AddCurrency(sellPrice);
            Debug.Log($"Vendeu {itemToSell.item.name} por {sellPrice}");
        }

        private int GetSellPrice(ItemSO item)
        {
            ShopItem? shopItem = ShopSO.ShopItems.Find(x => x.item == item);
            if (shopItem.HasValue)
            {
                return Mathf.FloorToInt(shopItem.Value.price * 0.5f);
            }
            return 10;
        }

        private int GetCurrencyAmount()
        {
            var inventoryState = playerInventory.GetCurrentInventoryState();
            int total = 0;

            foreach (var item in inventoryState.Values)
            {
                if (item.item == currencyItem)
                    total += item.quantity;
            }

            return total;
        }


        private void RemoveCurrency(int amount)
        {
            var inventoryState = playerInventory.GetCurrentInventoryState();
            foreach (var kvp in inventoryState)
            {
                if (kvp.Value.item == currencyItem)
                {
                    playerInventory.RemoveItem(kvp.Key, amount);
                    break;
                }
            }
        }

        private void AddCurrency(int amount)
        {
            playerInventory.AddItem(currencyItem, amount);
        }
        private void AudioPlay(int Music)
        {
            AudioS.resource = AudioSounds[Music];
            AudioS.Play();


        }
    }
}
