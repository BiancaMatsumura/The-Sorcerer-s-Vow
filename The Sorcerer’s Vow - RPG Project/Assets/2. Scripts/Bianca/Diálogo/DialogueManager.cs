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
        public bool startsQuestOnEnd;      // inicia quest ao finalizar o diálogo "antes"
        public bool completesQuestOnEnd;   // completa quest ao finalizar o diálogo "durante"
    }

    [Header("Lista de Quests controladas por este NPC")]
    public List<QuestDialogue> questDialogues = new();

    [Header("UI de interação")]
    public GameObject interactionUI;

    private QuestManager questManager;
    private bool playerInRange;
    private Transform mainCamera;
    private QuestDialogue activeQuestDialogue;

    private void Awake()
    {
        questManager = FindFirstObjectByType<QuestManager>();
        if (questManager == null)
            Debug.LogError("❌ Nenhum QuestManager encontrado na cena!");
    }

    private void Start()
    {
        mainCamera = Camera.main != null ? Camera.main.transform : null;
        if (interactionUI != null)
            interactionUI.SetActive(false);

        // Quando o diálogo termina, tenta iniciar/completar quest
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

    private void StartDialogue()
    {
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
        }
        else
        {
            switch (quest.QuestStatus)
            {
                case QuestStatus.Inactive:
                    conversationToStart = activeQuestDialogue.beforeQuestConversation;
                    break;
                case QuestStatus.Active:
                    conversationToStart = activeQuestDialogue.duringQuestConversation;
                    break;
                case QuestStatus.Completed:
                    conversationToStart = activeQuestDialogue.afterQuestConversation;
                    break;
            }
        }

        if (conversationToStart != null)
        {
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

            // Se a quest não existe ou não está completa, ainda é relevante
            if (quest == null || quest.QuestStatus != QuestStatus.Completed)
                return qd;
        }

        // Se todas as quests foram completadas, retorna a última
        if (questDialogues.Count > 0)
            return questDialogues[questDialogues.Count - 1];

        return null;
    }


    private void OnDialogueEnded()
    {
        if (activeQuestDialogue == null) return;

        Quest quest = questManager.GetQuestByID(activeQuestDialogue.questID);

        // Caso a quest ainda não exista, pode iniciar
        if (activeQuestDialogue.startsQuestOnEnd && (quest == null || quest.QuestStatus == QuestStatus.Inactive))
        {
            questManager.StartQuest(activeQuestDialogue.questID);
            Debug.Log($"📜 Quest {activeQuestDialogue.questID} iniciada após diálogo!");
        }

        // Caso esteja ativa e o diálogo de "durante" finalize
        if (activeQuestDialogue.completesQuestOnEnd && quest != null && quest.QuestStatus == QuestStatus.Active)
        {
            quest.Complete();
            Debug.Log($"🏁 Quest {activeQuestDialogue.questID} completada após diálogo!");
        }

        activeQuestDialogue = null;
    }
}
