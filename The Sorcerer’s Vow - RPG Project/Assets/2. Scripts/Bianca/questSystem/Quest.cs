using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Inventory.Model;

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

    [SerializeField] private ItemSO questRequiredItem; // ✅ Item necessário para completar a quest

    private Animator animator;
    private GameObject player;

    private QuestSystem questSystem;
    private InventoryController inventoryController; // ✅ Referência ao InventoryController

    private int currentQuestIndex = 0;
    private bool isTransitioning = false;

    void Awake()
    {
        questSystem = Object.FindAnyObjectByType<QuestSystem>();
        inventoryController = Object.FindAnyObjectByType<InventoryController>(); 

        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        SubscribeToCurrentQuest();
        DialogueGameEvents.Instace.OnStartDialog += StartTalkingAnimation;
    }

    private void SubscribeToCurrentQuest()
    {
        if (IsValidIndex())
        {
            var saveScript = FindAnyObjectByType<_2._Scripts.Core.Engine.Service.SaveLoad.SaveLoadProgressScript>();
            if (saveScript != null)
            {
                saveScript.SaveCurrentPlayer();
            }
            
            Debug.Log($"Inscrevendo na quest: {questPairs[currentQuestIndex].questData.questName}");
            var currentQuest = questPairs[currentQuestIndex].questData;
            currentQuest.OnQuestCompleted -= HandleQuestCompletion;
            currentQuest.OnQuestCompleted += HandleQuestCompletion;

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
        
        var saveScript = FindAnyObjectByType<_2._Scripts.Core.Engine.Service.SaveLoad.SaveLoadProgressScript>();
        if (saveScript != null)
        {
            saveScript.SaveCurrentPlayer();
        }

        var completedQuest = questPairs[currentQuestIndex].questData;
        Debug.Log($"Quest '{completedQuest.questName}' completada!");
        textOutput.text = $"Quest '{completedQuest.questName}' completada com sucesso!";

        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;

        Invoke(nameof(AdvanceToNextQuest), 2f);
    }

    private void AdvanceToNextQuest()
    {
        if (IsValidIndex())
        {
            var currentQuest = questPairs[currentQuestIndex].questData;
            currentQuest.OnQuestCompleted -= HandleQuestCompletion;
        }

        currentQuestIndex++;
        Debug.Log($"Avançando para a quest índice: {currentQuestIndex}");

        if (IsValidIndex())
        {
            var nextQuest = questPairs[currentQuestIndex].questData;

            SubscribeToCurrentQuest();
            UpdateQuestList();

            if (questSystem != null)
            {
                questSystem.ActivateQuest(nextQuest);
            }

            var dialogData = GetCurrentDialogueData();
            if (dialogData != null)
            {
                DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
            }
        }

        isTransitioning = false;

        DialogueManager dm = Object.FindAnyObjectByType<DialogueManager>();
        if (dm != null)
        {
            dm.playerInRange = false;
        }
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
            HandlePlayerEnter();
        }
    }

    public void OnPlayerEnterRange()
    {
        if (!IsValidIndex() || isTransitioning) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance > 2f)
        {
            Debug.Log("Muito longe para iniciar diálogo.");
            return;
        }

        HandlePlayerEnter();
    }

    private void HandlePlayerEnter()
    {
        Debug.Log("Player entrou na área da quest");
        var dialogData = GetCurrentDialogueData();

        if (questSystem != null)
        {
            var currentQuest = questPairs[currentQuestIndex].questData;
            questSystem.ActivateQuest(currentQuest);
        }

        if (dialogData != null)
        {
            textOutput.text = "Aperte E para falar.\nBotão Esquerdo do mouse para pular a animação.\nAperte Q para o próximo Diálogo";

            DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
            DialogueGameEvents.Instace.OnFinishDialog += CheckQuest;

            // ✅ Só ativa animação se NPC não estiver andando ou correndo
            if (animator != null && !animator.GetBool("isWalking") && !animator.GetBool("isRunning"))
            {
                animator.SetBool("isTalking", true);
            }

            DialogueGameEvents.Instace.OnFinishDialog += StopTalkingAnimation;
            DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
        }
        else
        {
            // ✅ Só chama CheckQuest se tiver o item necessário
            if (inventoryController != null && inventoryController.InventoryData.HasItem(questRequiredItem))
            {
                CheckQuest();
            }
            else
            {
                Debug.Log("Não é possível completar a quest: item necessário não está no inventário.");
            }
        }
    }

    private void StopTalkingAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isTalking", false);
        }
        DialogueGameEvents.Instace.OnFinishDialog -= StopTalkingAnimation;
    }

    private void StartTalkingAnimation(DialogueDataSO dialogueData)
    {
        if (dialogueData == GetCurrentDialogueData() && animator != null)
        {
            // ✅ Só ativa animação se NPC não estiver andando ou correndo
            if (!animator.GetBool("isWalking") && !animator.GetBool("isRunning"))
            {
                animator.SetBool("isTalking", true);
                DialogueGameEvents.Instace.OnFinishDialog -= StopTalkingAnimation;
                DialogueGameEvents.Instace.OnFinishDialog += StopTalkingAnimation;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerExit();
        }
    }

    public void OnPlayerExitRange()
    {
        HandlePlayerExit();
    }

    private void HandlePlayerExit()
    {
        Debug.Log("Player saiu da área da quest");
        textOutput.text = "Se aproxime novamente!";

        if (animator != null)
        {
            animator.SetBool("isTalking", false);
        }

        DialogueGameEvents.Instace.OnFinishDialog -= StopTalkingAnimation;
        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
        DialogueGameEvents.Instace.PlayerExitedDialogueRange();

        UpdateQuestList();
    }

    private void OnDisable()
    {
        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
    }

    private void OnDestroy()
    {
        if (IsValidIndex())
        {
            var currentQuest = questPairs[currentQuestIndex].questData;
            currentQuest.OnQuestCompleted -= HandleQuestCompletion;
        }

        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
        DialogueGameEvents.Instace.OnStartDialog -= StartTalkingAnimation;
    }

    public int GetCurrentQuestIndex()
    {
        return currentQuestIndex;
    }

    public void SetCurrentQuestIndex(int index)
    {
        if (questPairs != null && index >= 0 && index < questPairs.Count)
        {
            currentQuestIndex = index;
            SubscribeToCurrentQuest();
            UpdateQuestList();
        }
    }
}
