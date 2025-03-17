using System.Collections;
using UnityEngine;
using Inventory.Model;

public class Item3D : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private float duration = 0.3f;

    [SerializeField]
    private MeshFilter meshFilter; // Novo para definir o modelo 3D

    [SerializeField]
    private MeshRenderer meshRenderer; // Para renderizar o modelo

    // Add a reference to a quest that this item might be associated with
    [SerializeField]
    private QuestData linkedQuest;

    private QuestSystem questSystem;

    private void Start()
    {
        if (InventoryItem != null && meshFilter != null)
        {
            meshFilter.mesh = InventoryItem.ItemMesh; // Usando um Mesh ao invés de Sprite
            meshRenderer.material = InventoryItem.ItemMaterial; // Definindo o material do item
        }
        // Find the quest system
        questSystem = Object.FindAnyObjectByType<QuestSystem>();
    }

    public bool CanPickup()
    {
        // If there's no linked quest or no quest system, allow pickup
        if (linkedQuest == null || questSystem == null)
            return true;
            
        // Check if the quest dependencies allow this item to be picked up
        return questSystem.CanCompleteQuest(linkedQuest);
    }

    public void DestroyItem()
    {
        if (!CanPickup())
        {
            return; // Don't proceed with pickup
        }

        GetComponent<Collider>().enabled = false;
        StartCoroutine(AnimateItemPickup());
    }

    private IEnumerator AnimateItemPickup()
    {
        audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale =
                Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }

}
