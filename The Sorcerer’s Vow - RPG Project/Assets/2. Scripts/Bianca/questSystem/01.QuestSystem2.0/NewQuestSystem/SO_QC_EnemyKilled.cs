using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "QC_EnemyKilled", menuName = "QuestSystem/Components/QC_EnemyKilled", order = 1)]
    public class SO_QC_EnemyKilled : SO_QuestComponent
    {
        [Header("Enemy Settings")]
        public int enemyID;
        public int killsNeeded; 
    }
}