using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

namespace Bianca.QuestSystem
{
    public enum QuestType
    {
        Main
    }

    public class QuestSystem : MonoBehaviour
    {
        public List<QuestData> quests;
        [SerializeField] public TextMeshProUGUI textOutput;
        public event Action OnAllQuestsCompleted;

        private QuestData activeQuest; // quest ativa

        void Start()
        {
            ResetQuests();

            // 🔹 Ativar a primeira quest automaticamente se existir
            if (quests.Count > 0)
            {
                ActivateQuest(quests[0]);
            }
            else
            {
                UpdateUI();
            }

            foreach (var quest in quests)
            {
                quest.OnQuestCompleted -= OnQuestCompletedHandler;
                quest.OnQuestCompleted += OnQuestCompletedHandler;
            }
        }

        void OnDestroy()
        {
            foreach (var quest in quests)
            {
                quest.OnQuestCompleted -= OnQuestCompletedHandler;
            }
        }

        private void OnQuestCompletedHandler()
        {
            // se a quest ativa foi concluída, ativa a próxima da lista
            if (activeQuest != null && activeQuest.isCompleted)
            {
                int index = quests.IndexOf(activeQuest);

                if (index >= 0 && index + 1 < quests.Count)
                {
                    ActivateQuest(quests[index + 1]);
                }
                else
                {
                    activeQuest = null; // nenhuma próxima
                    UpdateUI();
                    OnAllQuestsCompleted?.Invoke();
                }
            }
            else
            {
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            if (textOutput == null) return;

            if (activeQuest != null && !activeQuest.isCompleted && activeQuest.isDiscovered)
            {
                textOutput.text = $"<b>Quest Ativa:</b>\n{activeQuest.questName}";
            }
            else
            {
                textOutput.text = "Nenhuma quest ativa.";
            }

            if (EmptyQuest().Count == 0)
            {
                OnAllQuestsCompleted?.Invoke();
            }
        }

        public bool CheckQuests()
        {
            foreach (var quest in quests)
            {
                if (!quest.isCompleted)
                    return false;
            }

            OnAllQuestsCompleted?.Invoke();
            return true;
        }

        public List<string> EmptyQuest()
        {
            List<string> empty = new List<string>();
            foreach (var quest in quests)
            {
                if (!quest.isCompleted)
                    empty.Add(quest.questName);
            }
            return empty;
        }

        public bool CheckQuest(string questName)
        {
            foreach (var quest in quests)
            {
                if (quest.questName == questName)
                {
                    if (CanCompleteQuest(quest))
                    {
                        if (!quest.isCompleted)
                        {
                            quest.CompleteQuest();
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        textOutput.text = $"A Quest '{questName}' possui dependências pendentes!";
                        return false;
                    }
                }
            }

            Debug.LogWarning($"Quest '{questName}' não encontrada!");
            return false;
        }

        public bool CanCompleteQuest(QuestData quest)
        {
            if (quest.questDependencies == null || quest.questDependencies.Count == 0)
                return true;

            foreach (var dependency in quest.questDependencies)
            {
                if (!dependency.isCompleted)
                    return false;
            }

            return true;
        }

        public void ResetQuests()
        {
            foreach (var quest in quests)
            {
                quest.isCompleted = false;
            }
            activeQuest = null;
        }

        public void NotifyItemCollected(QuestData quest)
        {
            if (quest != null && CanCompleteQuest(quest) && !quest.isCompleted)
            {
                quest.CompleteQuest();
            }
        }

        public void ActivateQuest(QuestData quest)
        {
            if (quest == null) return;

            if (!quests.Contains(quest))
            {
                quests.Add(quest);
            }

            quest.isDiscovered = true;
            activeQuest = quest; // define a quest ativa

            UpdateUI();

            Debug.Log($"Quest '{quest.questName}' ativada como ativa!");
        }

        public List<QuestData> GetQuestsByType(QuestType type)
        {
            return quests.FindAll(q => q.questType == type);
        }
    }
}
