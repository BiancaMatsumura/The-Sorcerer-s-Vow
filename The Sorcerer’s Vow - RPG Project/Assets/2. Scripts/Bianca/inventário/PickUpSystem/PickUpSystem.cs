using UnityEngine;
using Inventory.Model;
using TMPro;
using UnityEngine.UI;
using QuestSystem;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField] private InventorySO inventoryData;

    [SerializeField] private GameObject pickUpUI;
    [SerializeField] private TextMeshProUGUI nameItemUI;
    [SerializeField] private Image spriteItemUI;
    [SerializeField] private TextMeshProUGUI itemquantity;
    public Animation introanimation;

    private void OnTriggerEnter(Collider other)
    {
        Item3D item = other.GetComponent<Item3D>();
        if (item == null)
            return;

        // 🚫 Se não pode pegar, sai ANTES de qualquer coisa
        if (!item.CanPickup())
        {
            Debug.Log($"[PickUpSystem] Coleta bloqueada por missão: {item.InventoryItem.Name}");
            return;
        }

        // UI opcional
        nameItemUI.text = item.InventoryItem.Name;
        spriteItemUI.sprite = item.InventoryItem.ItemImage;
        itemquantity.text = item.Quantity.ToString();
        ShowPickUpUI();

        // ✔ Agora pode coletar automaticamente
        item.DestroyItem();
    }

    private void ShowPickUpUI()
    {
        if (pickUpUI != null)
        {
            pickUpUI.SetActive(true);
            introanimation.Rewind();
        }
    }
}
