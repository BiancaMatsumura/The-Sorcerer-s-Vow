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
            // Inicia a primeira quest manualmente (ex: tutorial)
            StartQuest(0);
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

            Quest quest = Quests[id];
            if (quest.QuestStatus != QuestStatus.Inactive) return false;

            quest.Activate();
            quest.OnQuestCompleted += QuestCompleted;

            Debug.Log($"▶ Iniciando quest: {quest.QuestName}");
            return true;
        }

        public void QuestCompleted(Quest quest)
        {
            Debug.Log($"🏁 Quest concluída: {quest.QuestName}");

            // Desinscreve o evento de conclusão
            quest.OnQuestCompleted -= QuestCompleted;

            // Dispara evento global de conclusão
            QuestEvents.TriggerQuestCompleted(quest);

            // 🔥 Ativa as próximas quests encadeadas
            SO_Quest questSO = questsToLoad.Find(q => q.id == quest.QuestID);
            if (questSO != null && questSO.nextQuests != null)
            {
                foreach (var nextSO in questSO.nextQuests)
                {
                    if (nextSO == null) continue;

                    if (Quests.TryGetValue(nextSO.id, out Quest nextQuest))
                    {
                        if (nextQuest.QuestStatus == QuestStatus.Inactive)
                        {
                            // Espera 1 frame antes de ativar (evita conflito de evento antigo)
                            StartCoroutine(ActivateNextQuestDelayed(nextQuest));
                        }
                    }
                }
            }
        }

        private System.Collections.IEnumerator ActivateNextQuestDelayed(Quest nextQuest)
        {
            yield return null; // aguarda 1 frame
            StartQuest(nextQuest.QuestID);
            Debug.Log($"➡ Próxima quest '{nextQuest.QuestName}' ativada automaticamente!");
        }

        private void InitializeQuests()
        {
            if (questsToLoad.Count <= 0) return;

            Quests = new Dictionary<int, Quest>();

            foreach (var questToLoad in questsToLoad)
            {
                Quest quest = new Quest(questToLoad.questName, questToLoad.id, questToLoad.components);
                Quests.Add(quest.QuestID, quest);
                Debug.Log($"📦 Quest '{quest.QuestName}' inicializada");
            }
        }

        
    }
}
