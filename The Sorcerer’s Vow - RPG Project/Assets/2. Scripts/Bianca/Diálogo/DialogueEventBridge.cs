using UnityEngine;
using DialogueEditor;

/// <summary>
/// Ponte entre o Dialogue Editor e o sistema de eventos customizado.
/// Converte os eventos do ConversationManager em eventos do DialogueGameEvents.
/// IMPORTANTE: Este script deve estar ativo na cena desde o início!
/// </summary>
public class DialogueEventBridge : MonoBehaviour
{
    private void Awake()
    {
        // Garante que o DialogueGameEvents existe
        if (DialogueGameEvents.Instance == null)
        {
            Debug.LogError("[DialogueEventBridge] DialogueGameEvents não encontrado na cena!");
        }
    }

    private void OnEnable()
    {
        // Inscreve nos eventos do Dialogue Editor
        ConversationManager.OnConversationStarted += OnDialogueStarted;
        ConversationManager.OnConversationEnded += OnDialogueEnded;
        
        Debug.Log("[DialogueEventBridge] ✅ Bridge ativado e escutando eventos do ConversationManager");
    }

    private void OnDisable()
    {
        // Remove inscrições para evitar memory leaks
        ConversationManager.OnConversationStarted -= OnDialogueStarted;
        ConversationManager.OnConversationEnded -= OnDialogueEnded;
    }

    private void OnDialogueStarted()
    {
        if (DialogueGameEvents.Instance == null) return;

        Debug.Log("[DialogueEventBridge] ✅ Diálogo INICIADO");
        
        // Não precisamos passar a conversa, só notificar que iniciou
        // O Quest.cs já sabe qual conversa é pela sua própria lógica
    }

    private void OnDialogueEnded()
    {
        if (DialogueGameEvents.Instance == null) return;

        Debug.Log("[DialogueEventBridge] ✅ Diálogo FINALIZADO");
        
        // CRÍTICO: Aqui dispara o evento que vai chamar Quest.CheckQuest()
        DialogueGameEvents.Instance.FinishDialog();
    }
}