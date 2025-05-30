using System.Collections;
using UnityEngine;
using Inventory.Model;

public class Item3D : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    public AudioSource audioSource;

    [SerializeField]
    private float duration = 0.3f;

    [SerializeField]
    private QuestData linkedQuest;

    public Quest quesCheck;
    private InventoryController inventoryController;

    private void Awake()
    {
        inventoryController = Object.FindAnyObjectByType<InventoryController>();
    }


    public void DestroyItem()
    {
        if (!CanPickup())
        {
            return;
        }

        if (inventoryController.InventoryData.CanAddItem(InventoryItem, Quantity))
        {
            GetComponent<Collider>().enabled = false;
            StartCoroutine(AnimateItemPickup());
        }
        else
        {
            Debug.Log("Não é possível coletar: inventário cheio.");
        }

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
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }

        if (quesCheck != null)
        {
            quesCheck.CheckQuest();
        }

        yield return new WaitForSeconds(2.2f);

        var questSystem = Object.FindAnyObjectByType<QuestSystem>();
        if (questSystem != null && linkedQuest != null)
        {
            questSystem.NotifyItemCollected(linkedQuest);
        }

        Destroy(gameObject);
    }

    public bool CanPickup()
    {
        var questSystem = Object.FindAnyObjectByType<QuestSystem>();
        if (linkedQuest == null || questSystem == null)
            return true;

        return questSystem.CanCompleteQuest(linkedQuest);
    }
}
