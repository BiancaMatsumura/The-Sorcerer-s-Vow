using UnityEngine;

public class Quest : MonoBehaviour
{
    public string nameQuest;

    private QuestSystem questSystem = null;

    void Awake()
    {
        // Se não foi atribuído no Inspector, então busca na cena
        if (questSystem == null)
        {
            questSystem = Object.FindFirstObjectByType<QuestSystem>();
        }
    }

    public void CheckQuest()
    {
        if(questSystem != null)
        {
            if(questSystem.CheckQuest(nameQuest) == true)
            {
                Debug.Log("Quest integralizada com sucesso!");
            }
            else
            {
                Debug.Log("A Quest não foi encontrada");
            }
        }
    }
}
