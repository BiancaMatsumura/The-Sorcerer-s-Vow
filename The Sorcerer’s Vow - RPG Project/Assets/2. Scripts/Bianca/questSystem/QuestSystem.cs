using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;

public class QuestSystem : MonoBehaviour
{
    public List<QuestData> quests;
    [SerializeField] public TextMeshProUGUI textOutput;
    public event Action OnAllQuestsCompleted;

    void Start()
    {
        ResetQuests();

        var questsFaltando = EmptyQuest();
        textOutput.text = questsFaltando.Count > 0
            ? $"Quest List:\n{string.Join("\n", questsFaltando)}"
            : "Todas as quests foram concluídas!";

        foreach (var quest in quests)
        {
            quest.OnQuestCompleted -= UpdateUI; // Evitar duplicação
            quest.OnQuestCompleted += UpdateUI;
        }
    }

    void OnDestroy()
    {
        foreach (var quest in quests)
        {
            quest.OnQuestCompleted -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        var questsFaltando = EmptyQuest();
        var newText = questsFaltando.Count > 0
            ? $"Quest List:\n{string.Join("\n", questsFaltando)}"
            : "Todas as quests foram concluídas. Level Finalizado!";

        if (textOutput != null && textOutput.text != newText)
        {
            textOutput.text = newText;
        }
        
        // Verificar se todas as quests foram concluídas
        if (questsFaltando.Count == 0)
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
                Debug.Log($"Verificando quest '{questName}', status atual: {(quest.isCompleted ? "Completa" : "Incompleta")}");
                
                if (CanCompleteQuest(quest))
                {
                    if (!quest.isCompleted)
                    {
                        quest.CompleteQuest();
                        Debug.Log($"Quest '{questName}' foi marcada como completa!");
                        UpdateUI();
                        return true;
                    }
                    else
                    {
                        Debug.Log($"Quest '{questName}' já está completa.");
                        return false; // A quest já está completa
                    }
                }
                else
                {
                    Debug.Log($"A Quest '{questName}' não pode ser concluída pois possui dependências pendentes!");
                    textOutput.text = $"A Quest '{questName}' não pode ser concluída pois possui dependências pendentes!";
                    return false;
                }
            }
        }
        Debug.LogWarning($"Quest '{questName}' não encontrada no sistema!");
        return false;
    }

    public bool CanCompleteQuest(QuestData quest)
    {
        if (quest.questDependencies == null || quest.questDependencies.Count == 0)
            return true;

        foreach (var dependency in quest.questDependencies)
        {
            if (!dependency.isCompleted)
            {
                Debug.Log($"Dependência '{dependency.questName}' não está completa para a quest '{quest.questName}'");
                return false;
            }
        }
        return true;
    }

    public void ResetQuests()
    {
        foreach (var quest in quests)
        {
            quest.isCompleted = false;
        }
    }

    public void NotifyItemCollected(QuestData quest)
    {
        if (quest != null && CanCompleteQuest(quest) && !quest.isCompleted)
        {
            quest.CompleteQuest();
            UpdateUI();
        }
    }
}