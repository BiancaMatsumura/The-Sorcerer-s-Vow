using UnityEngine;
using TMPro;
using System.Linq;

public class Testes : MonoBehaviour
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] public TextMeshProUGUI textOutput;

    void Awake()
    {
        if (questSystem == null)
            questSystem = Object.FindAnyObjectByType<QuestSystem>();

        questSystem.OnAllQuestsCompleted += LevelComplete;
    }

    public void BtCheckQuest()
    {
        if (questSystem == null)
        {
            Debug.LogWarning("QuestSystem não encontrado!");
            return;
        }

        if (questSystem.CheckQuests())
        {
            textOutput.text = "Level Finalizado!";
        }
        else
        {
            var questsFaltando = questSystem.EmptyQuest();
            textOutput.text = questsFaltando.Count > 0
                ? $"Level não Finalizado! Faltam: {string.Join(", ", questsFaltando)}"
                : "Level não Finalizado!";
        }
    }

    private void LevelComplete()
    {
        textOutput.text = "Todas as Quests foram completadas! Level Finalizado!";
    }
}