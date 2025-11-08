using DialogueEditor;
using UnityEngine;


namespace QuestSystem
{
    [CreateAssetMenu(fileName = "QC_DialogueQuest", menuName = "QuestSystem/Components/QC_DialogueQuest", order = 3)    ]
    public class SO_QC_DialogueQuest : SO_QuestComponent
    {   
        [Header("Dialogue Quest Settings")]
        public GameObject dialogue;
    }
}
