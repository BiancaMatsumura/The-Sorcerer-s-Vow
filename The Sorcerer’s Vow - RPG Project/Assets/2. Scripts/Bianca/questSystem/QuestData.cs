using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    public List<QuestData> questDependencies;
    public bool isCompleted;
    public bool isDiscovered = true; 

    public QuestType questType = QuestType.Main; 

    public event Action OnQuestCompleted;

    public void CompleteQuest()
    {
        isCompleted = true;
        OnQuestCompleted?.Invoke();
    }
}
