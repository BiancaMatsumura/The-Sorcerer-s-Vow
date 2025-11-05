using UnityEngine;
using Inventory.Model;
using TMPro;
using UnityEngine.UI;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    [SerializeField] private GameObject pickUpUI;
    [SerializeField] private TextMeshProUGUI nameItemUI;
    [SerializeField] private Image spriteItemUI;
    public Animation introanimation;

    private void OnTriggerEnter(Collider other)
    {

        Item3D item = other.GetComponent<Item3D>();
        if (item != null)
        { 
            if (!item.CanPickup())
            {
                return; // Exit without picking up
            }

            nameItemUI.text = item.InventoryItem.Name;
            spriteItemUI.sprite = item.InventoryItem.ItemImage;
            ShowPickUpUI();

            int remainder = inventoryData.AddItem(item.InventoryItem, item.Quantity);
            if (remainder == 0)
                item.DestroyItem();
            else
                item.Quantity = remainder;

        }

    }

    private void ShowPickUpUI()
    {
        if (pickUpUI != null)
        {
            introanimation.Play();
            pickUpUI.SetActive(true);
            Invoke("HidePickUpUI", 3f);
        }
    }
    private void HidePickUpUI()
    {
        if (pickUpUI != null)
        {
            pickUpUI.SetActive(false);
        }
    }

}