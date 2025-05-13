using System;
using System.Collections.Generic;
using Inventory.Model;
using ShopSystem;
using UnityEngine;

public class UIShopPage : MonoBehaviour
{
    [SerializeField] private RectTransform shopPanelA;
    [SerializeField] private RectTransform shopPanelB;
    [SerializeField] private UIShopItem shopItemPrefab;

    private Shop shopReference;

    public void InitializeShopUI(Shop shop)
    {
        shopReference = shop;
        PopulateShopItems();
    }

    private void PopulateShopItems()
    {

        // Limpa itens anteriores dos dois painéis
        foreach (Transform child in shopPanelA)
            Destroy(child.gameObject);

        foreach (Transform child in shopPanelB)
            Destroy(child.gameObject);

        List<ShopItem> items = shopReference.ShopSO.ShopItems;

        for (int i = 0; i < items.Count && i < 8; i++) // Limita a 8 itens no total
        {
            ShopItem shopItem = items[i];
            RectTransform parentPanel = (i < 4) ? shopPanelA : shopPanelB;

            UIShopItem itemUI = Instantiate(shopItemPrefab, parentPanel);
            itemUI.SetData(
                shopItem.item.ItemImage,
                shopItem.price,
                i,
                HandleBuyItem
            );
            if (shopReference.HasItemBeenPurchased(i))
            {
                itemUI.SetUnavailable();
            }


        }
    }

    private void HandleBuyItem(int index)
    {
        shopReference.BuyItem(index);
        PopulateShopItems(); // Atualiza interface após compra
    }



}


