using TMPro;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestDialoguePair
{
    public QuestData questData;
    public DialogueDataSO dialogueData;
}

public class Quest : MonoBehaviour
{
    [SerializeField] public List<QuestDialoguePair> questPairs;
    [SerializeField] public TextMeshProUGUI textOutput;

    private QuestSystem questSystem;
    private int currentQuestIndex = 0;
    private bool isTransitioning = false;

    void Awake()
    {
        questSystem = Object.FindAnyObjectByType<QuestSystem>();

        // Inscreve no evento da primeira quest
        SubscribeToCurrentQuest();
    }

    private void SubscribeToCurrentQuest()
    {
        if (IsValidIndex())
        {
            Debug.Log($"Inscrevendo na quest: {questPairs[currentQuestIndex].questData.questName}");
            var currentQuest = questPairs[currentQuestIndex].questData;
            currentQuest.OnQuestCompleted -= HandleQuestCompletion;
            currentQuest.OnQuestCompleted += HandleQuestCompletion;
            
            // Garantir que a quest está no sistema
            if (questSystem != null && !questSystem.quests.Contains(currentQuest))
            {
                questSystem.quests.Add(currentQuest);
            }
        }
    }

    private bool IsValidIndex()
    {
        return questPairs != null && currentQuestIndex >= 0 && currentQuestIndex < questPairs.Count;
    }

    private void HandleQuestCompletion()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        var completedQuest = questPairs[currentQuestIndex].questData;
        Debug.Log($"Quest '{completedQuest.questName}' completada!");
        textOutput.text = $"Quest '{completedQuest.questName}' completada com sucesso!";
        
        // Cancelar inscrição no evento OnFinishDialog antes de avançar
        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
        
        Invoke(nameof(AdvanceToNextQuest), 2f);
    }

    private void AdvanceToNextQuest()
    {
        // Desinscreve da quest atual antes de avançar
        if (IsValidIndex())
        {
            var currentQuest = questPairs[currentQuestIndex].questData;
            currentQuest.OnQuestCompleted -= HandleQuestCompletion;
        }
        
        currentQuestIndex++;
        Debug.Log($"Avançando para a quest índice: {currentQuestIndex}");

        if (IsValidIndex())
        {
            SubscribeToCurrentQuest();
            UpdateQuestList();
            
            var dialogData = GetCurrentDialogueData();
            if (dialogData != null)
            {
                DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
            }
        }

        
        isTransitioning = false;
    }

    private void UpdateQuestList()
    {
        if (questSystem != null)
        {
            questSystem.UpdateUI();
        }
    }

    public void CheckQuest()
    {
        if (questSystem == null || !IsValidIndex() || isTransitioning) return;
        
        var quest = questPairs[currentQuestIndex].questData;
        Debug.Log($"Verificando quest: {quest.questName}");
        
        if (questSystem.CheckQuest(quest.questName))
        {
            Debug.Log($"Quest {quest.questName} concluída com sucesso!");
            // Cancelar inscrição no evento para evitar múltiplas chamadas
            DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
            HandleQuestCompletion();
        }
    }

    private DialogueDataSO GetCurrentDialogueData()
    {
        if (IsValidIndex())
            return questPairs[currentQuestIndex].dialogueData;

        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && IsValidIndex() && !isTransitioning)
        {
            Debug.Log("Player entrou na área da quest");
            var dialogData = GetCurrentDialogueData();

            if (dialogData != null)
            {
                textOutput.text = "Aperte E para falar.\nBotão Esquerdo do mouse para pular a animação.\nAperte Q para o próximo Diálogo";
                
                // Remover inscrição anterior antes de inscrever novamente
                DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
                DialogueGameEvents.Instace.OnFinishDialog += CheckQuest;
                
                DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
            }
            else
            {
                CheckQuest();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player saiu da área da quest");
            textOutput.text = "Se aproxime novamente!";
            
            // Remover inscrição no evento ao sair
            DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
            DialogueGameEvents.Instace.PlayerExitedDialogueRange();
            
            UpdateQuestList();
        }
    }

    private void OnDisable()
    {
        // Remover inscrição no evento quando o objeto for desativado
        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
    }

    private void OnDestroy()
    {
        // Limpar todas as inscrições
        if (IsValidIndex())
        {
            var currentQuest = questPairs[currentQuestIndex].questData;
            currentQuest.OnQuestCompleted -= HandleQuestCompletion;
        }
        
        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
    }
}