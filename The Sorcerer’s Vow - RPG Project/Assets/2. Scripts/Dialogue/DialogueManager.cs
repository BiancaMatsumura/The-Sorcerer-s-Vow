// DialogueManager.cs
using System;
using System.Collections;
using System.Collections.Generic; // Needed for List
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Needed for Button and Image

/// <summary>
/// Manages the game's dialogue system, displaying text, handling node flow,
/// choices, conditions, and maintaining compatibility with the legacy sentence list.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image charImage; // Character image (optional)
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private DialogBar dialogBar; // Dialogue background/bar
    [SerializeField] private DialogueText dialogueText; // Component that shows text char by char
    [SerializeField] private RectTransform choicesContainer; // Where choice buttons will be instantiated
    [SerializeField] private Button choiceButtonPrefab; // Choice button prefab

    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private KeyCode nextSentenceKey = KeyCode.Q; // Or Mouse0, etc.
    [SerializeField] private KeyCode skipAnimationDialogue = KeyCode.Space;

    // Current dialogue state
    private StateDialogue state;
    // The DialogueDataSO being executed
    private DialogueDataSO currentDialogue;
    // Reference to the current node (new system)
    private DialogueNode currentNode;
    // Index of the current sentence (old system)
    private int currentSentenceIndex_Legacy = 0;
    // Flag to know if we are in legacy mode
    private bool isLegacyMode = false;
    // Stores the GUID of the next node for SentenceNode
    private string nextNodeGuidAfterSentence = null;
    // List to hold instantiated choice buttons
    private List<Button> currentChoiceButtons = new List<Button>();

    // Flag to know if the player is in range (FIXED: Added declaration)
    public bool playerInRange = false;

    void Start()
    {
        playerInRange = false; // Initialize playerInRange to false

        state = StateDialogue.disabled;
        // Hide UI initially
        HideDialogue(); // Ensure UI is hidden at start
        choicesContainer?.gameObject.SetActive(false); // Hide choice container

        // Subscribe to global events
        if (DialogueGameEvents.Instace != null)
        {
            //DialogueGameEvents.Instace.OnStartDialog += HandheldStartDialogueInternal; // Renamed for clarity
            DialogueGameEvents.Instace.OnPlayerEnteredDialogueRange += HandlePlayerEnteredRange;
            DialogueGameEvents.Instace.OnPlayerExitedDialogueRange += HandlePlayerExitedRange;
        }
        else
        {
            Debug.LogError("DialogueGameEvents.Instace not found! Event system is required.");
        }
    }

    public bool IsDialogueActive()
    {
        return state != StateDialogue.disabled;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (DialogueGameEvents.Instace != null)
        {
            DialogueGameEvents.Instace.OnStartDialog -= HandheldStartDialogueInternal;
            DialogueGameEvents.Instace.OnPlayerEnteredDialogueRange -= HandlePlayerEnteredRange;
            DialogueGameEvents.Instace.OnPlayerExitedDialogueRange -= HandlePlayerExitedRange;
        }
    }

    private void HandlePlayerEnteredRange(DialogueDataSO dialogueData)
    {
        currentDialogue = dialogueData;
        playerInRange = true; // Set flag when player enters range
        // Optional: Show interaction prompt (e.g., "Press E to talk")
    }

    private void HandlePlayerExitedRange()
    {
        playerInRange = false; // Clear flag when player exits range
        if (state != StateDialogue.disabled) // If a dialogue was active
        {
            FinishDialogue(); // End the dialogue if the player leaves
        }
        currentDialogue = null;
        // Optional: Hide interaction prompt
    }

    // Internal method called by OnStartDialog event or by Update
    private void HandheldStartDialogueInternal(DialogueDataSO dialogueData)
    {
        if (state != StateDialogue.disabled) return; // Already in dialogue

        currentDialogue = dialogueData; // Ensure we have the correct dialogue data
        isLegacyMode = false; // Assume new node system by default
        currentNode = null;
        currentSentenceIndex_Legacy = 0;
        nextNodeGuidAfterSentence = null;

        // Clear previous UI state
        ClearChoiceButtons();
        nameText.SetText("");
        dialogueText.HideText();
        if (charImage != null) charImage.gameObject.SetActive(false); // Hide image if used
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

        // Decide which system to use based on the DialogueDataSO content
        if (currentDialogue.EntryNode != null)
        {
            isLegacyMode = false;
            currentNode = currentDialogue.EntryNode;
            dialogBar?.Enable(); // Show the dialogue bar/background
            state = StateDialogue.processingNode; // Set state to process the first node
            ProcessNode(currentNode); // Start processing the entry node
        }
        else if (currentDialogue.Sentences != null && currentDialogue.Sentences.Count > 0)
        {
            isLegacyMode = true; // Use legacy system
            Debug.LogWarning($"Dialogue '{currentDialogue.name}' using legacy format (List<DialogSentence>).");
            dialogBar?.Enable(); // Show the dialogue bar/background
            state = StateDialogue.typing; // Use typing/waiting states for legacy mode
            ShowNextLegacySentence(); // Show the first legacy sentence
        }
        else
        {
            Debug.LogError($"Dialogue '{currentDialogue.name}' has no EntryNode and no Sentences. Cannot start.");
            state = StateDialogue.disabled; // Cannot start, remain disabled
        }
    }

    void Update()
    {
        // 1. Start Dialogue with Interaction Key
        // Check if player is in range, interaction key is pressed, a dialogue is available, and not already in dialogue
        if (playerInRange && Input.GetKeyDown(interactionKey) && currentDialogue != null && state == StateDialogue.disabled)
        {
            Debug.Log($"Starting dialogue: {currentDialogue.name}");
            HandheldStartDialogueInternal(currentDialogue);
        }

        // 2. Advance Sentence (Node System or Legacy System)
        // Check if next sentence key is pressed, we are waiting for input, and a dialogue is active
        if (Input.GetKeyDown(nextSentenceKey) && state == StateDialogue.waiting && currentDialogue != null)
        {
            if (isLegacyMode)
            {
                // Advance in legacy mode
                currentSentenceIndex_Legacy++;
                state = StateDialogue.typing; // Change state back to typing for the next sentence
                ShowNextLegacySentence(); // Display the next legacy sentence
            }
            else
            {
                // Advance in node system (after a SentenceNode)
                // Check if we have a stored next node GUID from the previous SentenceNode
                if (!string.IsNullOrEmpty(nextNodeGuidAfterSentence))
                {
                    DialogueNode nextNode = currentDialogue.FindNodeByGuid(nextNodeGuidAfterSentence);
                    nextNodeGuidAfterSentence = null; // Clear the stored GUID
                    if (nextNode != null)
                    {
                        state = StateDialogue.processingNode; // Change state to process the next node
                        ProcessNode(nextNode); // Process the found node
                    }
                    else
                    {
                        // If the next node GUID was invalid or node not found
                        Debug.LogWarning($"Next node with GUID {nextNodeGuidAfterSentence} not found after SentenceNode. Ending dialogue.");
                        FinishDialogue();
                    }
                }
                else
                {
                    // If there was no next node GUID stored (end of this branch)
                    FinishDialogue();
                }
            }
        }

        // 3. Skip Text Animation
        // Check if skip key is pressed and text is currently typing
        if (Input.GetKeyDown(skipAnimationDialogue) && state == StateDialogue.typing)
        {
            dialogueText.SkipAnimation();
            state = StateDialogue.waiting; // ← FORÇA o estado para waiting!
        }
    }

    // --- New Node System Logic ---

    /// Processes the current DialogueNode based on its type.
    private void ProcessNode(DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogWarning("Attempted to process a null node. Ending dialogue.");
            FinishDialogue();
            return;
        }

        currentNode = node; // Update the current node reference
        state = StateDialogue.processingNode; // Intermediate state while deciding action
        ClearChoiceButtons(); // Ensure old buttons are gone
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false); // Hide choice container by default

        // Determine node type and call the appropriate processing method
        if (node is SentenceNode sentenceNode)
        {
            ProcessSentenceNode(sentenceNode);
        }
        else if (node is ChoiceNode choiceNode)
        {
            ProcessChoiceNode(choiceNode);
        }
        else if (node is ConditionNode conditionNode)
        {
            ProcessConditionNode(conditionNode);
        }
        else
        {
            Debug.LogError($"Unknown node type: {node.GetType()}. Ending dialogue.");
            FinishDialogue();
        }
    }

    /// Processes a SentenceNode: displays actor info and text.
    private void ProcessSentenceNode(SentenceNode node)
    {
        // Set actor name and potentially image
        if (node.ActorData != null)
        {
            nameText.SetText(node.ActorData.CharacterName);
            if (charImage != null)
            {
                if (node.ActorData.Sprite != null)
                {
                    charImage.sprite = node.ActorData.Sprite;
                    charImage.gameObject.SetActive(true);
                }
                else { if (charImage != null) charImage.gameObject.SetActive(false); }
            }
        }
        else
        {
            nameText.SetText(""); // No actor, clear name
            if (charImage != null) charImage.gameObject.SetActive(false);
        }

        // Store the GUID of the next node BEFORE starting the text display coroutine
        nextNodeGuidAfterSentence = node.NextNodeGuid;
        // Start displaying the text with typing animation
        StartCoroutine(ShowTextAndAdvance(node.Content));
    }

    /// Processes a ChoiceNode: displays prompt and available choices.
    private void ProcessChoiceNode(ChoiceNode node)
    {
        // Set actor name and potentially image (optional for choices)
        if (node.ActorData != null)
        {
            nameText.SetText(node.ActorData.CharacterName);
            if (charImage != null)
            {
                if (node.ActorData.Sprite != null) { charImage.sprite = node.ActorData.Sprite; charImage.gameObject.SetActive(true); }
                else { if (charImage != null) charImage.gameObject.SetActive(false); }
            }
        }
        else
        {
            nameText.SetText(""); // No actor, clear name
            if (charImage != null) charImage.gameObject.SetActive(false);
        }

        // Show the prompt text immediately (no typing animation)
        dialogueText.ShowImmediate(node.PromptContent); // Use the new immediate method
        state = StateDialogue.waitingForChoice; // Set state to wait for player input on choices

        // Prepare and display the choice buttons based on conditions
        ShowChoiceButtons(node.Choices);
    }

    /// Processes a ConditionNode: evaluates the condition and branches.
    private void ProcessConditionNode(ConditionNode node)
    {
        if (node.ConditionToCheck == null)
        {
            Debug.LogError($"ConditionNode '{node.name}' has no ConditionSO assigned! Defaulting to 'False' branch.");
            GoToNextNode(node.FalseNodeGuid); // Go to False branch if no condition is set
            return;
        }

        // Evaluate the assigned condition
        // !! IMPORTANT: You'll likely need to pass game state info here !!
        bool result = node.ConditionToCheck.Evaluate(/* Pass game state, player data, etc. */);

        // Navigate to the next node based on the evaluation result
        GoToNextNode(result ? node.TrueNodeGuid : node.FalseNodeGuid);
    }

    /// Helper method to navigate to the next node based on a target GUID.
    private void GoToNextNode(string targetGuid)
    {
        if (string.IsNullOrEmpty(targetGuid))
        {
            // If no target GUID, this branch ends
            FinishDialogue();
        }
        else
        {
            // Find the next node in the current dialogue asset
            DialogueNode nextNode = currentDialogue.FindNodeByGuid(targetGuid);
            if (nextNode != null)
            {
                // If found, process it immediately
                ProcessNode(nextNode);
            }
            else
            {
                // If node with the specified GUID is not found
                Debug.LogError($"Could not find node with GUID: {targetGuid}. Ending dialogue.");
                FinishDialogue();
            }
        }
    }

    /// Coroutine to display text (SentenceNode) and set state to waiting.
    private IEnumerator ShowTextAndAdvance(string content)
    {
        state = StateDialogue.typing; // Set state to typing
        // Call DialogueText component to display text and wait for it to finish/be skipped
        yield return dialogueText.ShowText(content);
        // Once text is fully displayed (or skipped), set state to wait for player input
        state = StateDialogue.waiting;
    }

    // --- Choice UI Management ---

    /// Creates and displays buttons for available player choices.
    private void ShowChoiceButtons(List<PlayerChoice> choices)
    {
        if (choicesContainer == null || choiceButtonPrefab == null)
        {
            Debug.LogError("Choices Container or Choice Button Prefab is not assigned in the DialogueManager!");
            state = StateDialogue.disabled; // Cannot proceed without UI elements
            FinishDialogue(); // Or handle error differently
            return;
        }

        ClearChoiceButtons(); // Remove any old buttons
        choicesContainer.gameObject.SetActive(true); // Make the container visible

        bool atLeastOneChoiceAvailable = false;
        foreach (PlayerChoice choice in choices)
        {
            // 1. Evaluate the choice's availability condition (if it has one)
            bool available = true; // Assume available by default
            if (choice.AvailabilityCondition != null)
            {
                // !! IMPORTANT: Pass necessary game state info here !!
                available = choice.AvailabilityCondition.Evaluate(/* Pass game state */);
            }

            // 2. If the choice is available, create and configure its button
            if (available)
            {
                atLeastOneChoiceAvailable = true;
                Button choiceButton = Instantiate(choiceButtonPrefab, choicesContainer);
                choiceButton.gameObject.SetActive(true);

                // Set the button's text
                TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.SetText(choice.ChoiceText);
                }
                else
                {
                    Debug.LogWarning($"Choice button prefab '{choiceButtonPrefab.name}' is missing a TMP_Text component in its children.");
                }

                // Store the target node GUID for the button's action
                string targetGuid = choice.TargetNodeGuid; // Capture GUID for the lambda

                // Set up the button's click listener
                choiceButton.onClick.AddListener(() => OnChoiceSelected(targetGuid));

                // Add the button to the list for later cleanup
                currentChoiceButtons.Add(choiceButton);
            }
        }

        // Optional: Select the first available button for UI navigation (keyboard/gamepad)
        if (currentChoiceButtons.Count > 0)
        {
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(currentChoiceButtons[0].gameObject);
        }
        else if (!atLeastOneChoiceAvailable && choices != null && choices.Count > 0) // If there were choices defined, but none were available
        {
            Debug.LogWarning($"No available choices in ChoiceNode '{currentNode.name}'. Ending dialogue.");
            // Decide what to do: end dialogue, go to a default fallback node, etc.
            FinishDialogue();
        }
        // If choices list itself was empty, the loop won't run, and nothing happens yet.
        // Consider if an empty choices list should also end the dialogue.
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning($"ChoiceNode '{currentNode?.name ?? "Unknown"}' has no choices defined. Ending dialogue.");
            FinishDialogue();
        }
    }

    /// Called when a player choice button is clicked.
    private void OnChoiceSelected(string targetNodeGuid)
    {
        // Ignore clicks if not in the correct state
        if (state != StateDialogue.waitingForChoice) return;

        // Clean up UI
        ClearChoiceButtons();
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);

        // Navigate to the selected node
        GoToNextNode(targetNodeGuid);
    }

    /// Destroys currently displayed choice buttons.
    private void ClearChoiceButtons()
    {
        foreach (Button button in currentChoiceButtons)
        {
            if (button != null) Destroy(button.gameObject);
        }
        currentChoiceButtons.Clear();
    }

    // --- Legacy System Logic (List<DialogSentence>) ---

    /// Starts processing dialogue using the old list format.
    private void ProcessLegacySentences(DialogueDataSO dialogueData)
    {
        currentSentenceIndex_Legacy = 0; // Reset index
        ShowNextLegacySentence(); // Show the first sentence
    }

    /// Displays the next sentence from the legacy list.
    private void ShowNextLegacySentence()
    {
        // Basic validation
        if (currentDialogue == null || currentDialogue.Sentences == null)
        {
            FinishDialogue();
            return;
        }

        // Check if there are more sentences in the list
        if (currentSentenceIndex_Legacy < currentDialogue.Sentences.Count)
        {
            state = StateDialogue.typing; // Set state to typing
            DialogSentence sentence = currentDialogue.Sentences[currentSentenceIndex_Legacy];

            // Update actor name and image (if available)
            if (sentence.ActorData != null)
            {
                nameText.SetText(sentence.ActorData.CharacterName);
                if (charImage != null)
                {
                    if (sentence.ActorData.Sprite != null) { charImage.sprite = sentence.ActorData.Sprite; charImage.gameObject.SetActive(true); }
                    else { if (charImage != null) charImage.gameObject.SetActive(false); }
                }
            }
            else
            {
                nameText.SetText(""); // Clear name if no actor
                if (charImage != null) charImage.gameObject.SetActive(false);
            }
            // Start displaying the text
            StartCoroutine(ShowLegacyTextAndAdvance(sentence.Content));
        }
        else
        {
            // No more sentences in the legacy list
            FinishDialogue();
        }
    }

    /// Coroutine specific to legacy mode text display.
    private IEnumerator ShowLegacyTextAndAdvance(string content)
    {
        state = StateDialogue.typing;
        yield return dialogueText.ShowText(content); // Wait for text animation
        state = StateDialogue.waiting; // Set state to wait for next input key
    }

    // --- Common Methods ---

    /// Finishes the current dialogue, cleans up UI, and resets state.
    private void FinishDialogue()
    {
        // Avoid finishing multiple times
        if (state == StateDialogue.disabled) return;

        StopAllCoroutines(); // Stop any running text display coroutines

        // Reset state variables
        state = StateDialogue.disabled;
        currentNode = null;
        isLegacyMode = false;
        nextNodeGuidAfterSentence = null;
        // Do not clear currentDialogue here, HandlePlayerExitedRange does that

        // Clean up UI elements
        nameText.SetText("");
        dialogueText.HideText();
        dialogBar?.Disable();
        ClearChoiceButtons();
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        if (charImage != null) charImage.gameObject.SetActive(false);

        // Notify other systems that the dialogue finished
        DialogueGameEvents.Instace?.FinishDialog();
    }

    /// Immediately hides all dialogue UI elements and resets state.
    private void HideDialogue()
    {
        // Stop coroutines if any were running (e.g., called by HandlePlayerExitedRange)
        StopAllCoroutines();

        state = StateDialogue.disabled;
        currentNode = null;
        isLegacyMode = false;
        nextNodeGuidAfterSentence = null;

        nameText.SetText("");
        dialogueText.HideText();
        dialogBar?.Disable(); // Ensure bar is hidden/reset
        ClearChoiceButtons();
        if (choicesContainer != null) choicesContainer.gameObject.SetActive(false);
        if (charImage != null) charImage.gameObject.SetActive(false);
    }
}

// Enum representing the different states the dialogue manager can be in.
public enum StateDialogue
{
    disabled,         // No dialogue active, waiting for interaction
    typing,           // Displaying text character by character (node or legacy)
    waiting,          // Text fully displayed, waiting for player input to advance (node or legacy sentence)
    processingNode,   // Intermediate state while the manager decides how to handle the current node
    waitingForChoice  // Choice node prompt displayed, options shown, waiting for player button click
}
