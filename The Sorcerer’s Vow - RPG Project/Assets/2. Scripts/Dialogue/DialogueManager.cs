using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerencia o sistema de diálogo do jogo, exibindo o texto, controlando o fluxo das sentenças
/// e interagindo com a entrada do jogador.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image charImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private DialogBar dialogBar;
    [SerializeField] private DialogueText dialogueText;

    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private KeyCode nextSentenceKey = KeyCode.Space;

    StateDialogue state; 
    private DialogueDataSO currentDialogue; 
    private bool playerInRange = false; 
    private int currentSentenceIndex = 0; 

    void Start()
    {
        state = StateDialogue.disabled; 
        DialogueGameEvents.Instace.OnStartDialog += HandheldStartDiolog;
        DialogueGameEvents.Instace.OnPlayerEnteredDialogueRange += HandlePlayerEnteredRange;
        DialogueGameEvents.Instace.OnPlayerExitedDialogueRange += HandlePlayerExitedRange;
    }

    // Chamado quando o jogador entra na área de trigger do NPC.
    private void HandlePlayerEnteredRange(DialogueDataSO dialogueData)
    {
        currentDialogue = dialogueData;
        playerInRange = true;
        //Debug.Log("Player entrou no alcance do diálogo (DialogueManager).");
    }

    // Chamado quando o jogador sai da área de trigger do NPC.
    private void HandlePlayerExitedRange()
    {
        playerInRange = false; // Define que o jogador não está mais dentro do alcance.
        currentDialogue = null; // Limpa o diálogo atual.
        StopAllCoroutines(); // Para todas as corrotinas em execução.
        HideDialogue(); // Esconde os elementos visuais do diálogo.
        //Debug.Log("Player saiu do alcance do diálogo (DialogueManager).");
    }

    // Inicia o diálogo quando o evento OnStartDialog é disparado.
    private void HandheldStartDiolog(DialogueDataSO dialogueData)
    {
        //charImage.enabled = false;
        currentSentenceIndex = 0; // Reseta o índice da sentença.
        nameText.SetText(""); // Limpa o texto do nome.
        dialogBar.Enable(); // Habilita a barra de diálogo.

        StartCoroutine(StartDialog(dialogueData)); // Inicia a corrotina StartDialog.
    }

    // Corrotina para controlar o fluxo do diálogo.
    private IEnumerator StartDialog(DialogueDataSO dialogueData)
    {
        ShowNextSentence(); // Exibe a próxima sentença.
        yield return null; // Espera até a próxima frame.
    }

    // Exibe a próxima sentença do diálogo.
    private void ShowNextSentence()
    {
        if (currentDialogue == null) // Verifica se há um diálogo definido.
        {
            return;
        }

        if (currentSentenceIndex < currentDialogue.Sentences.Count && state == StateDialogue.disabled) // Verifica se há mais sentenças e se o diálogo está habilitado.
        {
            state = StateDialogue.typing; // Define o estado como digitando.
            var sentence = currentDialogue.Sentences[currentSentenceIndex]; // Pega a sentença atual.
            nameText.SetText(sentence.ActorData.CharacterName); // Define o nome do personagem.
            StartCoroutine(ShowTextAndAdvance(sentence.Content)); // Inicia a corrotina ShowTextAndAdvance.
        }
        else
        {
            FinishDialogue(); // Se não houver mais sentenças, finaliza o diálogo.
        }
    }

    // Corrotina para exibir o texto da sentença com animação.
    private IEnumerator ShowTextAndAdvance(string content)
    {
        state = StateDialogue.typing; // Define o estado como digitando.
        yield return dialogueText.ShowText(content); // Exibe o texto com animação e espera até que termine.
        state = StateDialogue.waiting; // Define o estado como esperando.
    }

    // Finaliza o diálogo.
    private void FinishDialogue()
    {
        state = StateDialogue.disabled; // Define o estado como desabilitado.
        nameText.SetText(""); // Limpa o texto do nome.
        dialogueText.HideText(); // Esconde o texto do diálogo.
        dialogBar.Disable(); // Desabilita a barra de diálogo.
        DialogueGameEvents.Instace.FinishDialog(); // Dispara o evento OnFinishDialog.
        currentDialogue = null; // Limpa o diálogo atual.
    }

    // Esconde os elementos visuais do diálogo.
    private void HideDialogue()
    {
        state = StateDialogue.disabled; // Define o estado como desabilitado.
        nameText.SetText(""); // Limpa o texto do nome.
        dialogueText.HideText(); // Esconde o texto do diálogo.
        dialogBar.Disable(); // Desabilita a barra de diálogo.
    }

    // Remove a inscrição dos eventos quando o objeto é destruído.
    private void OnDestroy()
    {
        DialogueGameEvents.Instace.OnStartDialog -= HandheldStartDiolog; // Remove a inscrição do evento OnStartDialog.
        DialogueGameEvents.Instace.OnPlayerEnteredDialogueRange -= HandlePlayerEnteredRange; // Remove a inscrição do evento OnPlayerEnteredDialogueRange.
        DialogueGameEvents.Instace.OnPlayerExitedDialogueRange -= HandlePlayerExitedRange; // Remove a inscrição do evento OnPlayerExitedDialogueRange.
    }

    void Update()
    {
        // Verifica se o jogador está dentro do alcance, se a tecla de interação foi pressionada, se há um diálogo e se o diálogo está desabilitado.
        if (playerInRange && Input.GetKeyDown(interactionKey) && currentDialogue != null && state == StateDialogue.disabled)
        {
            HandheldStartDiolog(currentDialogue); // Inicia o diálogo.
        }

        // Verifica se a tecla de avanço foi pressionada, se o estado é esperando e se há um diálogo.
        if (Input.GetKeyDown(nextSentenceKey) && state == StateDialogue.waiting && currentDialogue != null)
        {
            state = StateDialogue.disabled; // Define o estado como desabilitado para permitir a próxima sentença.
            currentSentenceIndex++; // Avança para a próxima sentença.
            ShowNextSentence(); // Exibe a próxima sentença.
        }

        // Se a tecla espaço for pressionada e o estado for digitando, pula a animação
        if (Input.GetKeyDown(KeyCode.Space) && state == StateDialogue.typing)
        {
            dialogueText.SkipAnimation();
        }
    }
}

// Enum para representar os diferentes estados do diálogo.
public enum StateDialogue
{
    disabled,
    waiting,
    typing
}