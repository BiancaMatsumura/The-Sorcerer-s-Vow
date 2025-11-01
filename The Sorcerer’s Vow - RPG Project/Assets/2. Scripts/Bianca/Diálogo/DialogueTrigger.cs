using UnityEngine;
using DialogueEditor;

[RequireComponent(typeof(Collider))]
public class DialogueTrigger : MonoBehaviour
{
    [Header("Configurações de Diálogo")]
    public NPCConversation conversation;
    public Quest linkedQuest;

    [Header("UI de Interação")]
    public GameObject interactionUI; // ex: um texto "Pressione E para conversar"

    private bool playerInRange;

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionUI != null)
                interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }

   
}
