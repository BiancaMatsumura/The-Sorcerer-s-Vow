using UnityEngine;

public class VerificarQuest : MonoBehaviour
{
    private QuestSystem questSystem;

    void Start()
    {
        if(questSystem == null)
        {
            questSystem = Object.FindFirstObjectByType<QuestSystem>();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            transform.GetComponent<Quest>().CheckQuest();
        }
    }
}
