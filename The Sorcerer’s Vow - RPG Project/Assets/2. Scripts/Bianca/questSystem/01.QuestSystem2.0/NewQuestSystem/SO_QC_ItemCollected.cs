using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "QC_ItemCollected", menuName = "QuestSystem/Components/QC_ItemCollected", order = 2)]
    public class SO_QC_ItemCollected : SO_QuestComponent
    {
        [Header("Item Settings")]
        public int itemID;
        public int itemAmountNeeded;
    }
}
