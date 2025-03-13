using UnityEngine;
using System.Collections.Generic;

public class QuestSystem : MonoBehaviour
{
    public List<QuestData> quests;


    void Start()
    {
        ResetQuests(); // Reseta todas as quests quando o jogo começa
    }

    public bool CheckQuests()
    {
        foreach (var quest in quests)
        {
            if (!quest.isCompleted)
                return false;
        }
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
                    quest.isCompleted = true;
                    return true;
                }
                else
                {
                    Debug.LogWarning($"A Quest '{questName}' não pode ser concluída pois possui dependências pendentes!");
                    return false;
                }
            }
        }
        return false;
    }

    public bool CanCompleteQuest(QuestData quest)
    {
        // Se a quest não tem dependências, ela pode ser concluída
        if (quest.questDependencies == null || quest.questDependencies.Count == 0)
            return true;

        // Verifica se todas as quests dependentes já foram concluídas
        foreach (var dependency in quest.questDependencies)
        {
            if (!dependency.isCompleted)
                return false; // Se uma das dependências não estiver concluída, bloqueia
        }

        return true; // Se todas as dependências foram cumpridas, libera
    }


    public void ResetQuests()
    {
        foreach (var quest in quests)
        {
            quest.isCompleted = false; // Reseta o progresso de todas as quests
        }
    }
}
