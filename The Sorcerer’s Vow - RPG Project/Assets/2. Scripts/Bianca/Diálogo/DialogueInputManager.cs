using UnityEngine;
using DialogueEditor;

public class DialogueInputManager : MonoBehaviour
{
    [Header("Teclas de diálogo configuráveis")]
    [SerializeField] private KeyCode keyPreviousOption = KeyCode.UpArrow;
    [SerializeField] private KeyCode keyNextOption = KeyCode.DownArrow;
    [SerializeField] private KeyCode keySelectOption = KeyCode.F;
    [SerializeField] private KeyCode keyAdvanceText = KeyCode.Q;
    [SerializeField] private KeyCode keySelectOptionAlt = KeyCode.Return; // alternativa para confirmar

    private void Update()
    {
        if (ConversationManager.Instance == null)
            return;

        if (!ConversationManager.Instance.IsConversationActive)
            return;

        // Seleciona opção anterior
        if (Input.GetKeyDown(keyPreviousOption))
        {
            ConversationManager.Instance.SelectPreviousOption();
        }

        // Seleciona próxima opção
        if (Input.GetKeyDown(keyNextOption))
        {
            ConversationManager.Instance.SelectNextOption();
        }

        // Confirma opção
        if (Input.GetKeyDown(keySelectOption) || Input.GetKeyDown(keySelectOptionAlt))
        {
            ConversationManager.Instance.PressSelectedOption();
        }

        // Avança texto/pula fala
        if (Input.GetKeyDown(keyAdvanceText))
        {
            ConversationManager.Instance.PressSelectedOption();
        }
    }
}
