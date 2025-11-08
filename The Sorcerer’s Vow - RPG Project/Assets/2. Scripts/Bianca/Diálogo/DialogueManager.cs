using UnityEngine;
using DialogueEditor;

[RequireComponent(typeof(Collider))]
public class DialogueManager : MonoBehaviour
{
    [Header("Diálogo padrão (fora de quest)")]
    public NPCConversation defaultConversation;

    [Header("UI de interação")]
    public GameObject interactionUI; // Ex: "Aperte E para conversar"

    public Quest quest; // ✅ Referência opcional para Quest.cs
    private bool playerInRange;

    private void Awake()
    {
        quest = GetComponent<Quest>();
    }

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

    private void Update()
    {
        if (!playerInRange) return;

        // Só permite iniciar se não estiver em um diálogo já ativo
        if (!ConversationManager.Instance.IsConversationActive && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        // Primeiro tenta diálogo da Quest
        NPCConversation activeConversation = null;

        if (quest != null)
        {
            activeConversation = quest.GetCurrentDialogueData();
        }

        // Se não há Quest ou diálogo da Quest, usa o padrão
        if (activeConversation == null)
            activeConversation = defaultConversation;

        if (activeConversation != null)
        {
            ConversationManager.Instance.StartConversation(activeConversation);
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{name} não tem diálogo configurado!");
        }
    }
}
