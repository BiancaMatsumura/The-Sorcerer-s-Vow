using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI_ : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject questobjectivePrefab;

    public Quest_ questTest;
    public int testQuestAmount;
    public List<QuestProgress> testQuests = new();

    void Start()
    {
        for (int i = 0; i < testQuestAmount; i++)
        {

            testQuests.Add(new QuestProgress(questTest));
        }

        UpdateQuestUI();
    }
    
    public void UpdateQuestUI()
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in testQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questTitle = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questTitle.text = quest.quest.questName;

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(questobjectivePrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description}: {objective.currentAmount}/{objective.requiredAmount}";
            }
        }
    }

}
