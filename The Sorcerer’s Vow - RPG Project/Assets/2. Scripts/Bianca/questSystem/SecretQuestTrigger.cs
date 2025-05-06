using UnityEngine;

public class SecretQuestTrigger : MonoBehaviour
{
    public QuestData secretQuest;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && secretQuest != null)
        {
            QuestSystem questSystem = FindFirstObjectByType<QuestSystem>();
            if (questSystem != null)
            {
                questSystem.ActivateQuest(secretQuest);
            }

            Destroy(gameObject); 
        }
    }
}
