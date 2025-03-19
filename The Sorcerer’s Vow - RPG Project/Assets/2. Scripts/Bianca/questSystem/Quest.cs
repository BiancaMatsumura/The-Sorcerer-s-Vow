using TMPro;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public QuestData questData;
    private QuestSystem questSystem;

    [SerializeField] public TextMeshProUGUI textOutput;

    void Awake()
    {
        questSystem = Object.FindAnyObjectByType<QuestSystem>();

        if (questData != null)
        {
            // Garantir que o evento OnQuestCompleted seja desinscrito antes de ser reinscrito
            questData.OnQuestCompleted -= HandleQuestCompletion; // Desinscrever, para evitar múltiplas assinaturas.
            questData.OnQuestCompleted += HandleQuestCompletion; // Inscrever novamente.
        }
    }

    private void HandleQuestCompletion()
    {
        // Exibir "Quest completada com sucesso!" primeiro
        textOutput.text = $"Quest '{questData.questName}' completada com sucesso!";

        // Aguardar 2 segundos antes de atualizar a lista de quests
        Invoke(nameof(UpdateQuestList), 2f);
    }

    private void UpdateQuestList()
    {
        // Atualiza a UI do QuestSystem somente após 2 segundos
        if (questSystem != null)
        {
            questSystem.UpdateUI();
        }
    }

    public void CheckQuest()
    {
        if (questSystem != null && questData != null)
        {
            if (questSystem.CheckQuest(questData.questName)) // Só processa se a quest foi concluída
            {
                HandleQuestCompletion(); // Garante que a mensagem de sucesso seja mostrada sempre
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckQuest();
        }
    }

    private void OnDestroy()
    {
        // Removendo o evento para evitar referências duplicadas
        if (questData != null)
        {
            questData.OnQuestCompleted -= HandleQuestCompletion;
        }
    }
}
