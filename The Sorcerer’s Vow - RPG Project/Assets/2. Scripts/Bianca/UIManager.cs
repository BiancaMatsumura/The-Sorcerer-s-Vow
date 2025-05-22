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

    public void ToggleInventory()
    {
        // Fecha Shop se estiver aberto
        if (shopUI.gameObject.activeSelf)
            CloseShop();

        if (!inventoryUI.isActiveAndEnabled)
        {
            inventoryUI.Show();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CameraController.isInventoryOpen = true;

            // Atualiza o inventário
            foreach (var kvp in inventoryData.GetCurrentInventoryState())
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
            inventoryUI.Hide();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            CameraController.isInventoryOpen = false;
        }
    }

    public void ToggleShop(Shop shop)
    {
        // Fecha Inventário se estiver aberto
        if (inventoryUI.isActiveAndEnabled)
            inventoryUI.Hide();

        if (!shopUI.gameObject.activeSelf)
        {
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
