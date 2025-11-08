using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activeQuests = new();
    private QuestUI_ questUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        questUI = Object.FindFirstObjectByType<QuestUI_>();
    }

    public void AcceptQuest(Quest_ quest)
    {
        if (IsQuestActive(quest.questID)) return;
        {
            activeQuests.Add(new QuestProgress(quest));
            questUI.UpdateQuestUI();
        }
        
    }

    public bool IsQuestActive(string questID) => activeQuests.Exists(q => q.quest.questID == questID);
}
