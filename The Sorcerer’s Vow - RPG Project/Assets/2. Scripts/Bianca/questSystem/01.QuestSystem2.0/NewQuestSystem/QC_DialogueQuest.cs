using DialogueEditor;
using UnityEngine;

namespace QuestSystem
{
    public class QC_DialogueQuest : QuestComponent
    {
        private GameObject _conversation;
        private bool _dialogueCompleted;

        public QC_DialogueQuest(string name, string description, GameObject conversation)
            : base(name, description)
        {
            _conversation = conversation;
            _dialogueCompleted = false;
            ComponentType = QuestComponentType.DialogueQuest;
        }

        public static QuestComponent CreateFactory(SO_QuestComponent so_questComponent)
        {
            SO_QC_DialogueQuest localQuestComponent = (SO_QC_DialogueQuest)so_questComponent;

            return new QC_DialogueQuest(
                localQuestComponent.componentName,
                localQuestComponent.description,
                localQuestComponent.dialogue);
        }

        public override void EnableComponent()
        {
            base.EnableComponent();
            Debug.Log($"{ComponentName} has been enabled.");

            // Começa a ouvir o evento do Dialogue Editor
            ConversationManager.OnConversationEnded += OnConversationEnded;
        }

        public override void MarkCompleted()
        {
            base.MarkCompleted();
            Debug.Log($"{ComponentName} has been completed.");

            // Remove a inscrição no evento
            ConversationManager.OnConversationEnded -= OnConversationEnded;
        }

        /// <summary>
        /// Inicia o diálogo desta quest.
        /// Deve ser chamado quando o jogador interagir com o NPC.
        /// </summary>
        public void StartDialogue()
        {
            if (_conversation == null)
            {
                Debug.LogWarning($"{ComponentName}: Nenhum diálogo foi atribuído!");
                return;
            }

            Debug.Log($"{ComponentName}: Diálogo iniciado ({_conversation.name})");
            ConversationManager.Instance.StartConversation(_conversation.GetComponent<NPCConversation>());
        }

        private void OnConversationEnded()
        {
            if (_dialogueCompleted)
                return;

            _dialogueCompleted = true;
            Debug.Log($"{ComponentName}: Diálogo foi concluído com sucesso!");

            MarkCompleted();
            TriggerComponentCompleted(this);
        }

        /// <summary>
        /// Caso o DialogueEditor não chame o evento OnConversationEnded,
        /// você pode chamar este método manualmente no final da conversa.
        /// </summary>
        public void CompleteDialogueQuest()
        {
            if (_dialogueCompleted)
                return;

            _dialogueCompleted = true;
            Debug.Log($"{ComponentName}: Diálogo forçado como concluído (manual).");

            MarkCompleted();
            TriggerComponentCompleted(this);
            
        }
    }
}
