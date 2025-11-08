using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Inventory.Model;
using DialogueEditor;
using System.Collections;
using Bianca.QuestSystem;


namespace Bianca.QuestSystem
{
[System.Serializable]
public class QuestDialoguePair
{
    [Header("Dados da Quest")]
    public QuestData questData;

    [Header("Diálogo (opcional)")]
    [Tooltip("Deixe vazio se a quest não tem diálogo")]
    public NPCConversation dialogueData;

    [Header("Item Requerido (opcional)")]
    [Tooltip("Item que o player precisa ter para completar esta quest")]
    public ItemSO requiredItem;

    [Header("Info")]
    [Tooltip("Apenas para organização no Inspector")]
    public string questNote = "";

}

    public class Quest : MonoBehaviour
    {
        [SerializeField] public List<QuestDialoguePair> questPairs;
        [SerializeField] public TextMeshProUGUI textOutput;
        [SerializeField] private ItemSO questRequiredItem;

        private Animator animator;
        private GameObject player;
        private QuestSystem questSystem;
        private InventoryController inventoryController;

        private int currentQuestIndex = 0;
        private bool isTransitioning = false;
        private bool playerInRange = false;

        void Awake()
        {
            Debug.Log($"[Quest] ========== AWAKE {gameObject.name} ==========");

            questSystem = Object.FindAnyObjectByType<QuestSystem>();
            inventoryController = Object.FindAnyObjectByType<InventoryController>();
            animator = GetComponent<Animator>();
            player = GameObject.FindGameObjectWithTag("Player");

            Debug.Log($"[Quest] QuestSystem found: {questSystem != null}");
            Debug.Log($"[Quest] InventoryController found: {inventoryController != null}");
            Debug.Log($"[Quest] Total questPairs: {questPairs?.Count ?? 0}");

            SubscribeToCurrentQuest();

            if (DialogueGameEvents.Instance != null)
            {
                DialogueGameEvents.Instance.OnStartDialog += StartTalkingAnimation;
                Debug.Log("[Quest] Subscrito ao OnStartDialog");
            }
            else
            {
                Debug.LogWarning("[Quest] DialogueGameEvents não encontrado no Awake!");
            }
        }

        private void SubscribeToCurrentQuest()
        {
            if (IsValidIndex())
            {
                Debug.Log($"[Quest] ========== SUBSCRIBING TO QUEST INDEX {currentQuestIndex} ==========");

                var currentQuest = questPairs[currentQuestIndex].questData;
                var currentDialogue = questPairs[currentQuestIndex].dialogueData;

                Debug.Log($"[Quest] Quest Name: {currentQuest.questName}");
                Debug.Log($"[Quest] Has Dialogue: {currentDialogue != null}");
                Debug.Log($"[Quest] Required Item: {(questRequiredItem != null ? questRequiredItem.Name : "NONE")}");

                currentQuest.OnQuestCompleted -= HandleQuestCompletion;
                currentQuest.OnQuestCompleted += HandleQuestCompletion;

                if (questSystem != null && !questSystem.quests.Contains(currentQuest))
                {
                    questSystem.quests.Add(currentQuest);
                    Debug.Log($"[Quest] Added to QuestSystem");
                }
            }
            else
            {
                Debug.LogError("[Quest] Invalid quest index in SubscribeToCurrentQuest!");
            }
        }

        private bool IsValidIndex()
        {
            bool valid = questPairs != null && currentQuestIndex >= 0 && currentQuestIndex < questPairs.Count;
            if (!valid)
            {
                Debug.LogError($"[Quest] INVALID INDEX! currentQuestIndex={currentQuestIndex}, questPairs count={questPairs?.Count ?? 0}");
            }
            return valid;
        }

        private void HandleQuestCompletion()
        {
            Debug.Log($"[Quest] ========== HANDLE QUEST COMPLETION ==========");

            if (isTransitioning)
            {
                Debug.LogWarning("[Quest] Already transitioning, ignoring completion");
                return;
            }

            isTransitioning = true;

            var completedQuest = questPairs[currentQuestIndex].questData;
            Debug.Log($"[Quest] ✅ Quest '{completedQuest.questName}' completada!");
            textOutput.text = $"Quest '{completedQuest.questName}' completada com sucesso!";

            if (DialogueGameEvents.Instance != null)
            {
                DialogueGameEvents.Instance.OnFinishDialog -= CheckQuest;
            }

            Invoke(nameof(AdvanceToNextQuest), 2f);
        }

        private void AdvanceToNextQuest()
        {
            Debug.Log($"[Quest] ========== ADVANCE TO NEXT QUEST ==========");

            if (IsValidIndex())
            {
                var currentQuest = questPairs[currentQuestIndex].questData;
                currentQuest.OnQuestCompleted -= HandleQuestCompletion;
                Debug.Log($"[Quest] Unsubscribed from quest {currentQuest.questName}");
            }

            currentQuestIndex++;
            Debug.Log($"[Quest] New quest index: {currentQuestIndex}");

            if (IsValidIndex())
            {
                var nextQuest = questPairs[currentQuestIndex].questData;
                var nextDialogue = questPairs[currentQuestIndex].dialogueData;

                Debug.Log($"[Quest] Next quest: {nextQuest.questName}");
                Debug.Log($"[Quest] Has dialogue: {nextDialogue != null}");
                Debug.Log($"[Quest] Required item: {(questRequiredItem != null ? questRequiredItem.Name : "NONE")}");

                SubscribeToCurrentQuest();
                UpdateQuestList();

                if (questSystem != null)
                {
                    questSystem.ActivateQuest(nextQuest);
                }

                // ✅ CRÍTICO: NÃO chamar PlayerEnteredDialogueRange aqui!
                if (textOutput != null)
                {
                    if (nextDialogue != null)
                    {
                        textOutput.text = "Nova quest disponível! Aproxime-se do NPC.";
                    }
                    else if (questRequiredItem != null)
                    {
                        textOutput.text = $"Nova quest: Procure e colete '{questRequiredItem.Name}'.";
                    }
                    else
                    {
                        textOutput.text = "Quest avançada!";
                    }
                }

                Debug.Log("[Quest] ✅ Advance complete - NOT calling any quest check");
            }
            else
            {
                Debug.Log("[Quest] 🎉 All quests completed!");
                if (textOutput != null)
                {
                    textOutput.text = "Todas as quests completadas!";
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
            Debug.Log($"[Quest] ========== CHECK QUEST CALLED ==========");
            Debug.Log($"[Quest] Stack trace: {System.Environment.StackTrace}");

            if (questSystem == null)
            {
                Debug.LogError("[Quest] ❌ QuestSystem is NULL!");
                return;
            }

            if (!IsValidIndex())
            {
                Debug.LogError("[Quest] ❌ Invalid quest index!");
                return;
            }

            if (isTransitioning)
            {
                Debug.LogWarning("[Quest] ⚠️ Already transitioning, BLOCKED");
                return;
            }

            var quest = questPairs[currentQuestIndex].questData;
            Debug.Log($"[Quest] Checking quest: '{quest.questName}'");
            Debug.Log($"[Quest] Quest already completed: {quest.isCompleted}");

            // ✅✅✅ VALIDAÇÃO CRÍTICA DO ITEM ✅✅✅
            if (questRequiredItem != null)
            {
                Debug.Log($"[Quest] 🔍 This quest REQUIRES item: '{questRequiredItem.Name}'");

                if (inventoryController == null)
                {
                    Debug.LogError("[Quest] ❌ InventoryController is NULL!");
                    return;
                }

                bool hasItem = inventoryController.InventoryData.HasItem(questRequiredItem);
                Debug.Log($"[Quest] Player has '{questRequiredItem.Name}': {hasItem}");

                if (!hasItem)
                {
                    Debug.LogError($"[Quest] ❌❌❌ BLOCKED: Player does NOT have '{questRequiredItem.Name}'!");
                    textOutput.text = $"Você ainda precisa coletar '{questRequiredItem.Name}'!";
                    return; // ✅ PARA AQUI!
                }

                Debug.Log($"[Quest] ✅ Validation passed: Player HAS '{questRequiredItem.Name}'");
            }
            else
            {
                Debug.Log("[Quest] No item required for this quest");
            }

            // Tenta completar
            Debug.Log($"[Quest] Calling questSystem.CheckQuest('{quest.questName}')...");
            bool completed = questSystem.CheckQuest(quest.questName);

            if (completed)
            {
                Debug.Log($"[Quest] ✅✅✅ Quest '{quest.questName}' COMPLETED!");

                if (DialogueGameEvents.Instance != null)
                {
                    DialogueGameEvents.Instance.OnFinishDialog -= CheckQuest;
                }
            }
            else
            {
                Debug.LogWarning($"[Quest] ❌ Quest '{quest.questName}' NOT completed (dependencies or already done)");
            }
        }

        public NPCConversation GetCurrentDialogueData()
        {
            if (IsValidIndex())
                return questPairs[currentQuestIndex].dialogueData;
            return null;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && IsValidIndex() && !isTransitioning)
            {
                playerInRange = true;
                Debug.Log($"[Quest] ========== PLAYER ENTERED TRIGGER ==========");
                HandlePlayerEnter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                Debug.Log($"[Quest] ========== PLAYER EXITED TRIGGER ==========");
                HandlePlayerExit();
            }
        }

        private void HandlePlayerEnter()
        {
            Debug.Log($"[Quest] HandlePlayerEnter - Index: {currentQuestIndex}");

            if (!IsValidIndex())
            {
                Debug.LogError("[Quest] Invalid index in HandlePlayerEnter!");
                return;
            }

            var dialogData = GetCurrentDialogueData();
            Debug.Log($"[Quest] DialogData is null: {dialogData == null}");
            Debug.Log($"[Quest] Required item: {(questRequiredItem != null ? questRequiredItem.Name : "NONE")}");

            if (questSystem != null)
            {
                var currentQuest = questPairs[currentQuestIndex].questData;
                questSystem.ActivateQuest(currentQuest);
            }

            if (dialogData != null)
            {
                // ✅ CASO 1: Quest com diálogo
                Debug.Log("[Quest] 💬 Quest has DIALOGUE");
                textOutput.text = "Aperte E para falar.\nBotão Esquerdo do mouse para pular a animação.\nAperte Q para o próximo Diálogo";

                if (DialogueGameEvents.Instance != null)
                {
                    DialogueGameEvents.Instance.OnFinishDialog -= CheckQuest;
                    DialogueGameEvents.Instance.OnFinishDialog += CheckQuest;
                    Debug.Log("[Quest] Subscribed to OnFinishDialog");
                }

                if (animator != null && !animator.GetBool("isWalking") && !animator.GetBool("isRunning"))
                {
                    animator.SetBool("isTalking", true);
                }

                if (DialogueGameEvents.Instance != null)
                {
                    DialogueGameEvents.Instance.OnFinishDialog += StopTalkingAnimation;
                    DialogueGameEvents.Instance.PlayerEnteredDialogueRange(dialogData);
                }
            }
            else if (questRequiredItem != null)
            {
                // ✅ CASO 2: Quest requer item
                Debug.Log("[Quest] 📦 Quest requires ITEM");

                if (inventoryController == null)
                {
                    Debug.LogError("[Quest] InventoryController is NULL!");
                    textOutput.text = "Sistema de inventário não disponível.";
                    return;
                }

                bool hasItem = inventoryController.InventoryData.HasItem(questRequiredItem);
                Debug.Log($"[Quest] Checking inventory for '{questRequiredItem.Name}': {hasItem}");

                if (hasItem)
                {
                    Debug.Log($"[Quest] ✅ Player HAS the item! Will complete quest.");
                    textOutput.text = $"✓ Item '{questRequiredItem.Name}' coletado!\nVerificando quest...";

                    // ✅ Completa após delay para garantir que item foi processado
                    StartCoroutine(DelayedCheckQuest());
                }
                else
                {
                    Debug.Log($"[Quest] ❌ Player does NOT have the item yet.");
                    textOutput.text = $"Procure e colete o item:\n'{questRequiredItem.Name}'";
                }
            }
            else
            {
                // ✅ CASO 3: Quest sem requisitos
                Debug.Log("[Quest] ⚠️ Quest has NO dialogue and NO item requirement");
                textOutput.text = "Sem tarefas no momento.";
            }
        }

        private IEnumerator DelayedCheckQuest()
        {
            Debug.Log("[Quest] Waiting 0.5s before checking quest...");
            yield return new WaitForSeconds(0.5f);
            CheckQuest();
        }

        private void StopTalkingAnimation()
        {
            if (animator != null)
            {
                animator.SetBool("isTalking", false);
            }

            if (DialogueGameEvents.Instance != null)
            {
                DialogueGameEvents.Instance.OnFinishDialog -= StopTalkingAnimation;
            }
        }

        private void StartTalkingAnimation(NPCConversation dialogueData)
        {
            if (dialogueData == GetCurrentDialogueData() && animator != null)
            {
                if (!animator.GetBool("isWalking") && !animator.GetBool("isRunning"))
                {
                    animator.SetBool("isTalking", true);
                    DialogueGameEvents.Instance.OnFinishDialog -= StopTalkingAnimation;
                    DialogueGameEvents.Instance.OnFinishDialog += StopTalkingAnimation;
                }
            }
        }

        private void HandlePlayerExit()
        {
            textOutput.text = "Se aproxime novamente!";

            if (animator != null)
            {
                animator.SetBool("isTalking", false);
            }

            if (DialogueGameEvents.Instance != null)
            {
                DialogueGameEvents.Instance.OnFinishDialog -= StopTalkingAnimation;
                DialogueGameEvents.Instance.OnFinishDialog -= CheckQuest;
                DialogueGameEvents.Instance.PlayerExitedDialogueRange();
            }

            UpdateQuestList();
        }

        private void OnDisable()
        {
            if (DialogueGameEvents.Instance != null)
            {
                DialogueGameEvents.Instance.OnFinishDialog -= CheckQuest;
            }
        }

        private void OnDestroy()
        {
            if (IsValidIndex())
            {
                var currentQuest = questPairs[currentQuestIndex].questData;
                if (currentQuest != null)
                {
                    currentQuest.OnQuestCompleted -= HandleQuestCompletion;
                }
            }

            if (DialogueGameEvents.Instance != null)
            {
                DialogueGameEvents.Instance.OnFinishDialog -= CheckQuest;
                DialogueGameEvents.Instance.OnStartDialog -= StartTalkingAnimation;
            }
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
}
