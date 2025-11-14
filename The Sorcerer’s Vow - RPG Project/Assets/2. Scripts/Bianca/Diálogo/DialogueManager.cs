using UnityEngine;
using DialogueEditor;
using QuestSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestDialogue
    {
        public int questID;
        public NPCConversation beforeQuestConversation;
        public NPCConversation duringQuestConversation;
        public NPCConversation afterQuestConversation;
        public bool startsQuestOnEnd;
        public bool completesQuestOnEnd;
    }

    [Header("Lista de Quests controladas por este NPC")]
    public List<QuestDialogue> questDialogues = new List<QuestDialogue>();

    [Header("UI de interação")]
    public GameObject interactionUI;

    private QuestManager questManager;
    private bool playerInRange;
    private Transform mainCamera;
    private QuestDialogue activeQuestDialogue;
    private NPCConversation currentConversation;

    private void Awake()
    {
        questManager = FindObjectOfType<QuestManager>(); // Corrigido
        if (questManager == null)
            Debug.LogError("❌ Nenhum QuestManager encontrado na cena!");
    }

    private void Start()
    {
        mainCamera = Camera.main != null ? Camera.main.transform : null;
        if (interactionUI != null)
            interactionUI.SetActive(false);

        ConversationManager.OnConversationEnded += OnDialogueEnded;
    }

    private void OnDestroy()
    {
        ConversationManager.OnConversationEnded -= OnDialogueEnded;
    }

    private void Update()
    {
        if (interactionUI != null && mainCamera != null)
            interactionUI.transform.LookAt(mainCamera);

        if (!playerInRange) return;

        if (!ConversationManager.Instance.IsConversationActive && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("✅ Player entrou na zona de interação"); // Debug adicionado
            if (interactionUI != null)
                interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("❌ Player saiu da zona de interação"); // Debug adicionado
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }

    private void StartDialogue()
    {
        Debug.Log("🎭 Tentando iniciar diálogo..."); // Debug adicionado
        
        activeQuestDialogue = GetRelevantQuestDialogue();
        if (activeQuestDialogue == null)
        {
            Debug.LogWarning($"⚠ {name} não tem nenhuma quest relevante no momento.");
            return;
        }

        Quest quest = questManager.GetQuestByID(activeQuestDialogue.questID);
        NPCConversation conversationToStart = null;

        if (quest == null)
        {
            conversationToStart = activeQuestDialogue.beforeQuestConversation;
            Debug.Log("📖 Iniciando diálogo BEFORE (quest não existe)");
        }
        else
        {
            switch (quest.QuestStatus)
            {
                case QuestStatus.Inactive:
                    conversationToStart = activeQuestDialogue.beforeQuestConversation;
                    Debug.Log("📖 Iniciando diálogo BEFORE (quest inativa)");
                    break;
                case QuestStatus.Active:
                    conversationToStart = activeQuestDialogue.duringQuestConversation;
                    Debug.Log("📖 Iniciando diálogo DURING (quest ativa)");
                    break;
                case QuestStatus.Completed:
                    conversationToStart = activeQuestDialogue.afterQuestConversation;
                    Debug.Log("📖 Iniciando diálogo AFTER (quest completa)");
                    break;
            }
        }

        if (conversationToStart != null)
        {
            currentConversation = conversationToStart;
            ConversationManager.Instance.StartConversation(conversationToStart);
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{name} não tem conversa configurada para o estado atual da quest {activeQuestDialogue.questID}!");
        }
    }

    private QuestDialogue GetRelevantQuestDialogue()
    {
        foreach (var qd in questDialogues)
        {
            Quest quest = questManager.GetQuestByID(qd.questID);
            if (quest == null || quest.QuestStatus != QuestStatus.Completed)
                return qd;
        }

        if (questDialogues.Count > 0)
            return questDialogues[questDialogues.Count - 1];

        return null;
    }

    private void OnDialogueEnded()
    {
        Debug.Log("🎬 Diálogo terminou!"); // Debug adicionado
        
        if (activeQuestDialogue == null || currentConversation == null)
        {
            Debug.Log("⚠ Nenhuma quest ativa para processar");
            return;
        }

        Quest quest = questManager.GetQuestByID(activeQuestDialogue.questID);

        if (currentConversation == activeQuestDialogue.beforeQuestConversation)
        {
            if (activeQuestDialogue.startsQuestOnEnd && (quest == null || quest.QuestStatus == QuestStatus.Inactive))
            {
                questManager.StartQuest(activeQuestDialogue.questID);
                Debug.Log($"📜 Quest {activeQuestDialogue.questID} iniciada após diálogo BEFORE!");
            }
        }

        if (currentConversation == activeQuestDialogue.duringQuestConversation)
        {
            if (activeQuestDialogue.completesQuestOnEnd && quest != null && quest.QuestStatus == QuestStatus.Active)
            {
                quest.Complete();
                Debug.Log($"🏁 Quest {activeQuestDialogue.questID} completada após diálogo DURING!");
            }
        }

        activeQuestDialogue = null;
        currentConversation = null;
    }

    public void StartDialogueExternal(NPCConversation conversationToStart)
    {
        ConversationManager.Instance.StartConversation(conversationToStart);
    }
}