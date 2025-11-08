using UnityEngine;
using QuestSystem;

public class TesteDialogoQuest : MonoBehaviour
{
    [Header("Referência da Quest de Diálogo")]
    [SerializeField] private QuestManager questManager; // arraste o QuestManager da cena aqui
    [SerializeField] private int questID; // ID da quest que tem o diálogo

    public void CompleteDialogueQuest()
    {
        // Busca a instância da quest ativa no QuestManager
        Quest questInstance = questManager.GetQuestByID(questID);
        if (questInstance == null)
        {
            Debug.LogWarning($"Nenhuma quest encontrada com ID {questID}.");
            return;
        }

        // Procura o componente de diálogo dentro da quest
        foreach (var component in questInstance.QuestComponents)
        {
            if (component is QC_DialogueQuest dialogueQuest)
            {

                QuestEvents.TriggerDialogueLineSpoken(questID);

                Debug.Log($"Diálogo da quest '{questInstance.QuestName}' marcado como concluído manualmente!");
                return;
            }

        }
        questManager.QuestCompleted(questInstance);

        Debug.LogWarning($"Nenhum componente de diálogo encontrado na quest '{questInstance.QuestName}'.");
    }
}
