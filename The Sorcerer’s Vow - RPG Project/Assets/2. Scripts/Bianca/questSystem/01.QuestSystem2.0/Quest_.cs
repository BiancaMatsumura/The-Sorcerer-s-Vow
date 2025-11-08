using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Quest System 2.0/Quest")]
public class Quest_ : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjective> objectives;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    public string description;

    public ObjectiveType type;
    public int requiredAmount;
    public int currentAmount;

    public enum ObjectiveType
    {
        Collect,
        Kill,
        ReachLocation,
        Talk,
        Custom
    }

    public bool IsCompleted()
    {
        return currentAmount >= requiredAmount;
    }
}

[System.Serializable]
public class QuestProgress
{
    public Quest_ quest;
    public List<QuestObjective> objectives;

    public QuestProgress(Quest_ quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();
        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsQuestCompleted => objectives.TrueForAll(o => o.IsCompleted());

    public string QuestID => quest.questID;

}
