using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 


/// <summary>
/// Gerencia o sistema de di�logo do jogo, exibindo o texto, controlando o fluxo das senten�as
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
    [SerializeField] private KeyCode nextSentenceKey = KeyCode.Q;
    [SerializeField] private KeyCode skipAnimationDialogue = KeyCode.Space;

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

    // Chamado quando o jogador entra na �rea de trigger do NPC.
    private void HandlePlayerEnteredRange(DialogueDataSO dialogueData)
    {
        currentDialogue = dialogueData;
        playerInRange = true;
    }

    // Chamado quando o jogador sai da �rea de trigger do NPC.
    private void HandlePlayerExitedRange()
    {
        playerInRange = false; // Define que o jogador n�o est� mais dentro do alcance.
        currentDialogue = null; // Limpa o di�logo atual.
        StopAllCoroutines(); // Para todas as corrotinas em execu��o.
        HideDialogue(); // Esconde os elementos visuais do di�logo.
    }

    // Inicia o di�logo quando o evento OnStartDialog � disparado.
    private void HandheldStartDiolog(DialogueDataSO dialogueData)
    {
        //charImage.enabled = false;
        currentSentenceIndex = 0; // Reseta o �ndice da senten�a.
        nameText.SetText(""); // Limpa o texto do nome.
        dialogBar.Enable(); // Habilita a barra de di�logo.

        StartCoroutine(StartDialog(dialogueData)); // Inicia a corrotina StartDialog.
    }

    // Corrotina para controlar o fluxo do di�logo.
    private IEnumerator StartDialog(DialogueDataSO dialogueData)
    {
        ShowNextSentence(); // Exibe a pr�xima senten�a.
        yield return null; // Espera at� a pr�xima frame.
    }

    // Exibe a pr�xima senten�a do di�logo.
    private void ShowNextSentence()
    {
        if (currentDialogue == null)
        {
            return;
        }

        if (currentSentenceIndex < currentDialogue.Sentences.Count && state == StateDialogue.disabled)
        {
            state = StateDialogue.typing; // Define o estado como digitando.
            var sentence = currentDialogue.Sentences[currentSentenceIndex]; // Pega a senten�a atual.
            nameText.SetText(sentence.ActorData.CharacterName); // Define o nome do personagem.
            StartCoroutine(ShowTextAndAdvance(sentence.Content)); // Inicia a corrotina ShowTextAndAdvance.
        }
        else
        {
            FinishDialogue(); // Se n�o houver mais senten�as, finaliza o di�logo.
        }
    }

    // Corrotina para exibir o texto da senten�a com anima��o.
    private IEnumerator ShowTextAndAdvance(string content)
    {
        state = StateDialogue.typing; // Define o estado como digitando.
        yield return dialogueText.ShowText(content); // Exibe o texto com anima��o e espera at� que termine.
        state = StateDialogue.waiting; // Define o estado como esperando.
    }

    // Finaliza o di�logo.
    private void FinishDialogue()
    {
        state = StateDialogue.disabled; // Define o estado como desabilitado.
        nameText.SetText(""); // Limpa o texto do nome.
        dialogueText.HideText(); // Esconde o texto do di�logo.
        dialogBar.Disable(); // Desabilita a barra de di�logo.
        DialogueGameEvents.Instace.FinishDialog(); // Dispara o evento OnFinishDialog.
    }

    // Esconde os elementos visuais do di�logo.
    private void HideDialogue()
    {
        state = StateDialogue.disabled;
        nameText.SetText(""); // Limpa o texto do nome.
        dialogueText.HideText(); // Esconde o texto do di�logo.
        dialogBar.Disable(); // Desabilita a barra de di�logo.
    }

    // Remove a inscri��o dos eventos quando o objeto � destru�do.
    private void OnDestroy()
    {
        DialogueGameEvents.Instace.OnStartDialog -= HandheldStartDiolog;
        DialogueGameEvents.Instace.OnPlayerEnteredDialogueRange -= HandlePlayerEnteredRange;
        DialogueGameEvents.Instace.OnPlayerExitedDialogueRange -= HandlePlayerExitedRange;
    }

    void Update()
    {
        // Verifica se o jogador est� dentro do alcance, se a tecla de intera��o foi pressionada, se h� um di�logo e se o di�logo est� desabilitado.
        if (playerInRange && Input.GetKeyDown(interactionKey) && currentDialogue != null && state == StateDialogue.disabled)
        {
            HandheldStartDiolog(currentDialogue);
        }

        // Verifica se a tecla de avan�o foi pressionada, se o estado � esperando e se h� um di�logo.
        if (Input.GetKeyDown(nextSentenceKey) && state == StateDialogue.waiting && currentDialogue != null)
        {
            state = StateDialogue.disabled;
            currentSentenceIndex++; // Avan�a para a pr�xima senten�a.
            ShowNextSentence(); // Exibe a pr�xima senten�a.
        }

        // Se a tecla espa�o for pressionada e o estado for digitando, pula a anima��o
        if (Input.GetKeyDown(skipAnimationDialogue) && state == StateDialogue.typing)
        {
            dialogueText.SkipAnimation();
        }
    }
}

// Enum para representar os diferentes estados do di�logo.
public enum StateDialogue
{
    disabled,
    waiting,
    typing
}