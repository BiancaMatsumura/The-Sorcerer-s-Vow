using UnityEngine;
using TMPro;

public class Testes : MonoBehaviour
{
    [SerializeField] private QuestSystem questSystem;
    [SerializeField] public TextMeshProUGUI textOutput;
    
    void Awake()
    {
        // Se não foi atribuído no Inspector, então busca na cena
        if (questSystem == null)
        {
            questSystem = Object.FindFirstObjectByType<QuestSystem>();
        }
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
            string[] questEmpty = questSystem.EmptyQuest();
            textOutput.text = questEmpty.Length > 0
                ? $"Level não Finalizado! Faltam os seguintes objetivos: {string.Join(", ", questEmpty)}"
                : "Level não Finalizado!";
        }
    }
}