using TMPro;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public QuestData questData;
    private QuestSystem questSystem;

    [SerializeField] public TextMeshProUGUI textOutput;

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
                textOutput.text = $"Quest '{questData.questName}' completada com sucesso!";
            }
            else
            {
                textOutput.text = $"Não foi possível completar a Quest '{questData.questName}'!";
            }
        }
    }
}
