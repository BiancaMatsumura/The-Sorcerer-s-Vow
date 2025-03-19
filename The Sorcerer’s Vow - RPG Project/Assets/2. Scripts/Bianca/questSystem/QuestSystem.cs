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
            ? $"Quest List: {string.Join(", ", questsFaltando)}"
            : "Todas as quests foram concluídas!";

        // Inscreve no evento de cada quest para atualizar a UI quando for completada
        foreach (var quest in quests)
        {
            quest.OnQuestCompleted += UpdateUI;
        }
    }

    void OnDestroy()
    {
        // Desinscrever todos os eventos para evitar múltiplas inscrições
        foreach (var quest in quests)
        {
            quest.OnQuestCompleted -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        var questsFaltando = EmptyQuest();
        var newText = questsFaltando.Count > 0
            ? $"Quest List: {string.Join(", ", questsFaltando)}"
            : "Todas as quests foram concluídas. Level Finalizado!";

        // Atualiza a UI somente se o texto mudou
        if (textOutput.text != newText)
        {
            textOutput.text = newText;
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
                    quest.CompleteQuest();
                    return true; // Retorna verdadeiro para permitir o delay no Quest.cs
                }
                else
                {
                    textOutput.text = $"A Quest '{questName}' não pode ser concluída pois possui dependências pendentes!";
                    return false;
                }
            }
        }
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
        // Verificar se a quest está associada a este item
        if (quest != null)
        {
            // Atualize a UI ou a lógica de dependência de quest conforme necessário
            quest.isCompleted = true; // Ou qualquer outra lógica de atualização de quest
            UpdateUI();  // Atualiza a UI do QuestSystem
        }
    }

}
