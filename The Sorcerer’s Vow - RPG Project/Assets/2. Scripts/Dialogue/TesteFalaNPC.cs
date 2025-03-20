using UnityEngine;

public class TesteFalaNPC : MonoBehaviour
{
    [SerializeField] DialogueDataSO dialogData;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player;
        if (other.TryGetComponent<PlayerController>(out player))
        {
            Debug.Log("Player está próximo!");
            DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData); // Notifica o DialogueManager
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player;
        if (other.TryGetComponent<PlayerController>(out player))
        {
            Debug.Log("Player se afastou!");
            DialogueGameEvents.Instace.PlayerExitedDialogueRange(); // Notifica o DialogueManager
        }
    }
}