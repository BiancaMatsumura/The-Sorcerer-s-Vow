using UnityEngine;
using Inventory.Model;
using ShopSystem;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIInventoryPage inventoryUI;
    [SerializeField] private UIShopPage shopUI;
    [SerializeField] private InventorySO inventoryData;

    private Shop activeShop;

    public bool IsInventoryOpen => inventoryUI.isActiveAndEnabled;
    public bool IsShopOpen => shopUI.gameObject.activeSelf;


    public void ToggleInventory()
    {
        // Close shop if it's open
        if (IsShopOpen)
        {
            CloseShop();
        }

        if (!IsInventoryOpen)
        {
            // Show inventory
            inventoryUI.Show();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CameraController.isInventoryOpen = true;

            // Populate inventory items
            var currentState = inventoryData.GetCurrentInventoryState();
            foreach (var kvp in currentState)
            {
                inventoryUI.UpdateData(
                    kvp.Key,
                    kvp.Value.item.ItemImage,
                    kvp.Value.quantity,
                    kvp.Value.item.Category
                );
            }
        }
        else
        {
            // Hide inventory
            inventoryUI.Hide();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            CameraController.isInventoryOpen = false;
        }
    }

    public void ToggleShop(Shop shop)
    {
        // Close inventory if it's open
        if (IsInventoryOpen)
        {
            ToggleInventory();
        }

        if (!IsShopOpen)
        {
            // Open shop
            activeShop = shop;
            shopUI.gameObject.SetActive(true);
            shopUI.InitializeShopUI(activeShop);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CameraController.isInventoryOpen = true;
        }
        else
        {
            // Close shop
            CloseShop();
        }
    }

    public void CloseShop()
    {
        shopUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraController.isInventoryOpen = false;
    }
}
