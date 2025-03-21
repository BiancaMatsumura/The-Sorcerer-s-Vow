using UnityEngine;

public class TesteFalaNPC : MonoBehaviour
{
    [SerializeField] DialogueDataSO dialogData;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player está próximo!");
            DialogueGameEvents.Instace.PlayerEnteredDialogueRange(dialogData);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player se afastou!"); 
            DialogueGameEvents.Instace.PlayerExitedDialogueRange();
        }
    }
}