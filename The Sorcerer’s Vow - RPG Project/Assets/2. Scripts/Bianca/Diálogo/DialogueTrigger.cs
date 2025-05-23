using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    private Quest quest;

    private void Awake()
    {
        quest = GetComponentInParent<Quest>();
        if (quest == null)
        {
            Debug.LogWarning("DialogueTrigger não encontrou Quest no pai!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("DialogueTrigger: Player entrou no range.");
            quest?.OnPlayerEnterRange();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("DialogueTrigger: Player saiu do range.");
            quest?.OnPlayerExitRange();
        }
    }
}
