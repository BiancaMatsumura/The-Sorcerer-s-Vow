using Inventory.Model;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoinCount : MonoBehaviour
{
    [SerializeField] private InventorySO playerInventory;
    [SerializeField] private ItemSO currencyItem;
    [SerializeField] private TextMeshProUGUI coinText;

    private int lastCoinAmount = -1;

    private void OnEnable()
    {
        playerInventory.OnInventoryUpdated += OnInventoryChanged;
        UpdateCoinCount();
    }

    private void OnDisable()
    {
        playerInventory.OnInventoryUpdated -= OnInventoryChanged;
    }


    private void OnInventoryChanged(Dictionary<int, InventoryItem> inventoryState)
    {
        UpdateCoinCount();
    }

    private void UpdateCoinCount()
    {
        int amount = GetCurrencyAmount();
        if (amount != lastCoinAmount)
        {
            lastCoinAmount = amount;
            coinText.text = amount.ToString();
        }
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

}
