using UnityEngine;
using Inventory.Model;
using ShopSystem;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIInventoryPage inventoryUI;
    [SerializeField] private UIShopPage shopUI;
    [SerializeField] private InventorySO inventoryData;
    [SerializeField] private QuestUI_ questUI;

    private Shop activeShop;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleQuest();
        }
    }

    public void ToggleInventory()
    {
        if (shopUI.gameObject.activeSelf)
            CloseShop();

        if (!inventoryUI.isActiveAndEnabled)
        {
            inventoryUI.Show();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CameraController.isInventoryOpen = true;

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
            CloseInventory();
        }
    }

    public void ToggleShop(Shop shop)
    {
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

    public void ToggleQuest()
    {
        if (questUI == null)
            return;

        bool isPanelActive = questUI.gameObject.activeSelf;

        if (isPanelActive)
        {
            CloseQuestPanel();
            return;
        }

        if (inventoryUI.isActiveAndEnabled)
            CloseInventory();

        if (shopUI.gameObject.activeSelf)
            CloseShop();

        questUI.Show();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CameraController.isInventoryOpen = true;
    }

    public void CloseShop()
    {
        shopUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraController.isInventoryOpen = false;
    }

    public void CloseInventory()
    {
        inventoryUI.Hide();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraController.isInventoryOpen = false;
    }

    public void CloseQuestPanel()
    {
        questUI.Hide();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraController.isInventoryOpen = false;
    }
}
