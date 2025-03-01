using UnityEngine;

public class PickUpSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    private void OnTriggerEnter(Collider other)
    {
        Item3D item = other.GetComponent<Item3D>();
        if (item != null)
        {
            int remainder = inventoryData.AddItem(item.InventoryItem, item.Quantity);
            if (remainder == 0)
                item.DestroyItem();
            else
                item.Quantity = remainder;
        }
    }
}
