using UnityEngine;

public class Quest : MonoBehaviour
{
    public QuestData questData;
    private QuestSystem questSystem;

    void Awake()
    {
        questSystem = Object.FindAnyObjectByType<QuestSystem>();
    }

    public void CheckQuest()
    {
        if (questSystem != null && questData != null)
        {
            if (questSystem.CheckQuest(questData.questName))
            {
                Debug.Log($"Quest '{questData.questName}' completada com sucesso!");
            }
            else
            {
                Debug.Log($"Não foi possível completar a Quest '{questData.questName}'!");
            }
        }
    }
}
