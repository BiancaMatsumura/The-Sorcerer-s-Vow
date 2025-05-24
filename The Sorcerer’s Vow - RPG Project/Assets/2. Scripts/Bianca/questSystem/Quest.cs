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

    Animator animator;
    private GameObject player;

    private QuestSystem questSystem;
    private int currentQuestIndex = 0;
    private bool isTransitioning = false;
    void Awake()
    {
        questSystem = Object.FindAnyObjectByType<QuestSystem>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        SubscribeToCurrentQuest();
        DialogueGameEvents.Instace.OnStartDialog += StartTalkingAnimation;
    }


    private void SubscribeToCurrentQuest()
    {
        if (IsValidIndex())
        {
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

        // ✅ FORÇA o playerInRange a false no DialogueManager
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
            Debug.Log("Player entrou na área da quest");
            var dialogData = GetCurrentDialogueData();

            if (questSystem != null && IsValidIndex())
            {
                var currentQuest = questPairs[currentQuestIndex].questData;
                questSystem.ActivateQuest(currentQuest);
            }

            if (dialogData != null)
            {
                textOutput.text = "Aperte E para falar.\nBotão Esquerdo do mouse para pular a animação.\nAperte Q para o próximo Diálogo";

                DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
                DialogueGameEvents.Instace.OnFinishDialog += CheckQuest;

                if (animator != null)
                {
                    animator.SetBool("isTalking", true);
                }

                DialogueGameEvents.Instace.OnFinishDialog += StopTalkingAnimation;
                DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
            }
            else
            {
                CheckQuest();
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
            animator.SetBool("isTalking", true);
            DialogueGameEvents.Instace.OnFinishDialog -= StopTalkingAnimation;
            DialogueGameEvents.Instace.OnFinishDialog += StopTalkingAnimation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player saiu da área da quest");
            textOutput.text = "Se aproxime novamente!";

            DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
            DialogueGameEvents.Instace.PlayerExitedDialogueRange();

            UpdateQuestList();
        }
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

    public void OnPlayerEnterRange()
    {
        if (!IsValidIndex() || isTransitioning) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance > 2f) // exemplo: 2 metros de distância mínima
        {
            Debug.Log("Muito longe para iniciar diálogo.");
            return;
        }

        Debug.Log("Quest: Player entrou na área.");

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

            if (animator != null)
            {
                animator.SetBool("isTalking", true);
            }

            DialogueGameEvents.Instace.OnFinishDialog += StopTalkingAnimation;
            DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
        }
        else
        {
            CheckQuest();
        }
    }


    public void OnPlayerExitRange()
    {
        Debug.Log("Quest: Player saiu da área.");
        textOutput.text = "Se aproxime novamente!";

        DialogueGameEvents.Instace.OnFinishDialog -= CheckQuest;
        DialogueGameEvents.Instace.PlayerExitedDialogueRange();

        UpdateQuestList();
    }

}
