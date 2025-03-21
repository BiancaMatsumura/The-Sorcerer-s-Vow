using System;
using UnityEngine;

/// <summary>
/// Gerencia eventos relacionados ao sistema de diálogos, permitindo a comunicação entre diferentes partes do jogo.
/// Utiliza o padrão Singleton para garantir uma única instância acessível globalmente.
/// </summary>
[DefaultExecutionOrder(-1)]
public class DialogueGameEvents : MonoBehaviour
{
    // Instância Singleton da classe DialogueGameEvents. Use DialogueGameEvents.Instace para acessar.
    public static DialogueGameEvents Instace { get; private set; }

    private void Awake()
    {
        Instace = this;
    }

    // Evento disparado quando um diálogo é iniciado. Passa o objeto DialogueSO como argumento.
    public event Action<DialogueDataSO> OnStartDialog;
    public void StartDialog(DialogueDataSO dialogueSO) => OnStartDialog?.Invoke(dialogueSO);

    // Evento disparado quando um diálogo é finalizado.
    public event Action OnFinishDialog;
    public void FinishDialog() => OnFinishDialog?.Invoke();

    // Evento para quando o jogador entra no alcance do diálogo
    public event Action<DialogueDataSO> OnPlayerEnteredDialogueRange;
    public void PlayerEnteredDialogueRange(DialogueDataSO dialogueSO) => OnPlayerEnteredDialogueRange?.Invoke(dialogueSO);

    // Evento para quando o jogador sai do alcance do diálogo
    public event Action OnPlayerExitedDialogueRange;
    public void PlayerExitedDialogueRange() => OnPlayerExitedDialogueRange?.Invoke();
}