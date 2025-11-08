using UnityEngine;

namespace QuestSystem
{
    public abstract class SO_QuestComponent : ScriptableObject
    {
        [Header("Component Settings")]
        public string componentName;
        public QuestComponent.QuestComponentType questType;
        [TextArea(2, 10)]
        public string description;
    }
}