using System.Collections.Generic;
using TMPro;
using UnityEngine;
using QuestSystem;

public class QuestUI_ : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject questUIPanel;
    [SerializeField] private Transform questListContent;
    [SerializeField] private GameObject questEntryPrefab;
    [SerializeField] private GameObject questObjectivePrefab;

    [Header("Referência do Sistema de Quests")]
    [SerializeField] private QuestManager questManager; // Arraste o QuestManager da cena


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            questUIPanel.SetActive(!questUIPanel.activeSelf);
        }
    }
    private void OnEnable()
    {
        // Atualiza a UI sempre que uma quest for concluída
        QuestEvents.OnQuestCompleted += OnQuestCompleted;
    }

    private void OnDisable()
    {
        QuestEvents.OnQuestCompleted -= OnQuestCompleted;
    }

    private void Start()
    {
        UpdateQuestUI();
    }

    private void OnQuestCompleted(Quest quest)
    {
        Debug.Log($"UI: Atualizando após concluir {quest.QuestName}");
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        // Limpa a lista atual
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Garante que o QuestManager está válido
        if (questManager == null || questManager.Quests == null)
        {
            Debug.LogWarning("QuestManager ou lista de Quests não atribuída.");
            return;
        }

        // Exibe todas as quests
        foreach (var questPair in questManager.Quests)
        {
            Quest quest = questPair.Value;

            // Cria o item da quest
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questTitle = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            // Título da quest
            string statusText = quest.QuestStatus.ToString();
            questTitle.text = $"{quest.QuestName} <color=#888888>({statusText})</color>";

            // Lista de componentes (objetivos)
            foreach (QuestComponent component in quest.QuestComponents)
            {
                GameObject objTextGO = Instantiate(questObjectivePrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();

                // Exibe o nome e o tipo do componente
                string compName = component.ComponentName;
                string compType = component.ComponentType.ToString();

                string status = (quest.QuestStatus == QuestStatus.Completed ||
                                 (component is QC_DialogueQuest dq && dq != null))
                                 ? "✔" : "•";

                objText.text = $"{status} {compName} <color=#888888>({compType})</color>";
            }
        }
    }
}
