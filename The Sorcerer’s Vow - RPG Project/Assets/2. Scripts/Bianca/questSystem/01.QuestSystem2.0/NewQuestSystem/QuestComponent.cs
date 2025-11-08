using System;
using UnityEngine;

namespace QuestSystem
{
    public abstract class QuestComponent
    {
        public enum QuestComponentType
        {
            EnemyKilled,
            ItemCollected,
            DialogueQuest
        }
        
        public event Action<QuestComponent> OnComponentCompleted;
        protected void TriggerComponentCompleted(QuestComponent questComponent) { OnComponentCompleted?.Invoke(questComponent); }

        public string ComponentName;
        public string ComponentDescription;
        public QuestComponentType ComponentType;

        public QuestComponent(string name, string description)
        {
            ComponentName = name;
            ComponentDescription = description;
        }

        public virtual void EnableComponent()
        {
            Debug.unityLogger.Log($"{ComponentName} has been enabled.");
        }

        public virtual void MarkCompleted()
        {
            Debug.unityLogger.Log($"{ComponentName} has been completed.");
        }
    }
}