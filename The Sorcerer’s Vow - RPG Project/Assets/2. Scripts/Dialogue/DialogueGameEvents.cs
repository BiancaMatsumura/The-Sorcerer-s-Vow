using System;
using UnityEngine;
using DialogueEditor;

/// <summary>
/// Gerencia eventos relacionados ao sistema de diálogos, permitindo a comunicação entre diferentes partes do jogo.
/// Utiliza o padrão Singleton para garantir uma única instância acessível globalmente.
/// </summary>
[DefaultExecutionOrder(-1)]
public class DialogueGameEvents : MonoBehaviour
{
    // ✅ CORRIGIDO: Instância Singleton da classe DialogueGameEvents
    public static DialogueGameEvents Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[DialogueGameEvents] Instância duplicada detectada! Destruindo...");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[DialogueGameEvents] ✅ Sistema de eventos inicializado");
    }

    // Evento disparado quando um diálogo é iniciado. Passa o objeto NPCConversation como argumento.
    public event Action<NPCConversation> OnStartDialog;
    public void StartDialog(NPCConversation dialogue)
    {
        Debug.Log($"[DialogueGameEvents] Disparando OnStartDialog para: {dialogue?.name}");
        OnStartDialog?.Invoke(dialogue);
    }

    // Evento disparado quando um diálogo é finalizado.
    public event Action OnFinishDialog;
    public void FinishDialog()
    {
        Debug.Log("[DialogueGameEvents] 🔔 Disparando OnFinishDialog");
        OnFinishDialog?.Invoke();
    }

    // Evento para quando o jogador entra no alcance do diálogo
    public event Action<NPCConversation> OnPlayerEnteredDialogueRange;
    public void PlayerEnteredDialogueRange(NPCConversation dialogue)
    {
        Debug.Log($"[DialogueGameEvents] Player entrou no alcance: {dialogue?.name}");
        OnPlayerEnteredDialogueRange?.Invoke(dialogue);
    }

    // Evento para quando o jogador sai do alcance do diálogo
    public event Action OnPlayerExitedDialogueRange;
    public void PlayerExitedDialogueRange()
    {
        Debug.Log("[DialogueGameEvents] Player saiu do alcance");
        OnPlayerExitedDialogueRange?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}