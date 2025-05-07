using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

public enum QuestType
{
    Main,
    Side,
    Secret
}


public class QuestSystem : MonoBehaviour
{
    public List<QuestData> quests;
    [SerializeField] public TextMeshProUGUI textOutput;
    public event Action OnAllQuestsCompleted;

    void Start()
    {
        ResetQuests();

        UpdateUI();

        foreach (var quest in quests)
        {
            quest.OnQuestCompleted -= UpdateUI;
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
        string main = FormatQuestList(QuestType.Main);
        string side = FormatQuestList(QuestType.Side);
        string secret = FormatQuestList(QuestType.Secret);

        string newText = $"<b>Main Quests:</b>\n{main}\n\n<b>Side Quests:</b>\n{side}\n\n<b>Secret Quests:</b>\n{secret}";

        if (textOutput != null && textOutput.text != newText)
        {
            textOutput.text = newText;
        }

        if (EmptyQuest().Count == 0)
        {
            OnAllQuestsCompleted?.Invoke();
        }
    }


    private string FormatQuestList(QuestType type)
    {
        var questsByType = quests
            .Where(q => q.questType == type && !q.isCompleted && q.isDiscovered)
            .Select(q => q.questName)
            .ToList();

        return questsByType.Count > 0 ? string.Join("\n", questsByType) : "Nenhuma";
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
                        UpdateUI();
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
    }

    public void NotifyItemCollected(QuestData quest)
    {
        if (quest != null && CanCompleteQuest(quest) && !quest.isCompleted)
        {
            quest.CompleteQuest();
            UpdateUI();
        }
    }


    public void ActivateQuest(QuestData quest)
    {
        if (quest != null && !quests.Contains(quest))
        {
            quests.Add(quest);
        }

        
        quest.isDiscovered = true;

        quest.OnQuestCompleted -= UpdateUI;
        quest.OnQuestCompleted += UpdateUI;
        UpdateUI();

        Debug.Log($"Quest '{quest.questName}' ativada e agora visível!");
    }



    public List<QuestData> GetQuestsByType(QuestType type)
    {
        return quests.FindAll(q => q.questType == type);
    }
}
