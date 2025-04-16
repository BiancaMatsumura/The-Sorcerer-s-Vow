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

    private QuestSystem questSystem;

    public Quest quesCheck;

    private void Start()
    {
       
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
            return; // Não coleta o item se não for permitido
        }

        // Referencia o componente Quest no mesmo GameObject
        Quest questComponent = GetComponent<Quest>();
        if (questComponent != null)
        {
            // Chama o método CompleteQuest diretamente no componente Quest
            questComponent.CheckQuest();
        }

        // Desativa o colisor para evitar que o item seja coletado novamente
        GetComponent<Collider>().enabled = false;

        if(quesCheck != null)
        {
            quesCheck.CheckQuest();
        }
        

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
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }

        // Agora, o item é destruído após a animação
        Destroy(gameObject);
    }


}
