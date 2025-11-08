using UnityEngine;

namespace QuestSystem
{
    public class QC_EnemyKilled : QuestComponent
    {
        private int _enemyID;
        private int _killCount;
        private int _killsNeeded;
    
        public QC_EnemyKilled(string name, string description, int enemyID, int killsNeeded) : base(name, description)
        {
            _enemyID = enemyID;
            _killsNeeded = killsNeeded;
            _killCount = 0;
            ComponentType = QuestComponentType.EnemyKilled;
        }
        
        public static QuestComponent CreateFactory(SO_QuestComponent so_questComponent)
        {
            SO_QC_EnemyKilled localQuestComponent = (SO_QC_EnemyKilled)so_questComponent;

            return new QC_EnemyKilled(
                localQuestComponent.componentName,
                localQuestComponent.description,
                localQuestComponent.enemyID,
                localQuestComponent.killsNeeded);
        }

        public override void EnableComponent()
        {
            base.EnableComponent();
            // Subscribe to enemy kill event
            QuestEvents.OnEnemyKilled += EnemyKilled;
        }

        public override void MarkCompleted()
        {
            base.MarkCompleted();
            // Unsubscribe from enemy kill event
            QuestEvents.OnEnemyKilled -= EnemyKilled;
        }
        
        private void EnemyKilled(int enemyID)
        {
            if (_enemyID != enemyID)
                return;

            _killCount++;
            
            Debug.unityLogger.Log($"{ComponentName}: Enemy Type {enemyID} was killed {_killCount}/{_killsNeeded}");

            if (_killCount < _killsNeeded)
                return;
            
            MarkCompleted();
            TriggerComponentCompleted(this);
        }
    }
}