using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private List<SO_Quest> questsToLoad = new List<SO_Quest>();

        public Dictionary<int, Quest> Quests;

        private static QuestManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            InitializeQuests();
        }

        private void Start()
        {
            StartQuest(0); // Inicia a quest com ID 1 ao começar o jogo
        }

        public Quest GetQuestByID(int id)
        {
            if (Quests == null) return null;
            Quests.TryGetValue(id, out Quest quest);
            return quest;
        }

        public bool StartQuest(int id)
        {
            if (!Quests.ContainsKey(id)) return false;
            if (Quests[id].QuestStatus != QuestStatus.Inactive) return false;

            Quests[id].Activate();
            Quests[id].OnQuestCompleted += QuestCompleted;

            Debug.unityLogger.Log($"{Quests[id].QuestName} has been started");
            return true;
        }

        public void QuestCompleted(Quest quest)
        {
            Debug.unityLogger.Log($"{quest.QuestName} has been completed");
            quest.OnQuestCompleted -= QuestCompleted;

            QuestEvents.TriggerQuestCompleted(quest);

            // 🔥 Ativa as próximas quests encadeadas
            SO_Quest questSO = questsToLoad.Find(q => q.id == quest.QuestID);
            if (questSO != null && questSO.nextQuests != null)
            {
                foreach (var next in questSO.nextQuests)
                {
                    if (next != null)
                    {
                        if (Quests.TryGetValue(next.id, out Quest nextQuest))
                        {
                            if (nextQuest.QuestStatus == QuestStatus.Inactive)
                            {
                                StartQuest(nextQuest.QuestID);
                                Debug.Log($"➡ Próxima quest '{nextQuest.QuestName}' foi ativada automaticamente!");
                            }
                        }
                    }
                }
            }
        }

        private void InitializeQuests()
        {
            if (questsToLoad.Count <= 0) return;

            Quests = new Dictionary<int, Quest>();

            for (var i = 0; i < questsToLoad.Count; i++)
            {
                SO_Quest questToLoad = questsToLoad[i];
                Quest quest = new Quest(questToLoad.questName, questToLoad.id, questToLoad.components);
                Quests.Add(quest.QuestID, quest);

                Debug.unityLogger.Log($"{quest.QuestName} has been initialized");
            }
        }
    }
}