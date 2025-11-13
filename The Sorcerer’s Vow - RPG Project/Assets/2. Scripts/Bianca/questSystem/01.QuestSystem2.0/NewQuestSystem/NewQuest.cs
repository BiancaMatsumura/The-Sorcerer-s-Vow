using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    public enum QuestStatus { Inactive, Active, Completed };
    // Inactive     - Quest has not been started
    // Active       - Quest is current in progress
    // Completed    - Quest has been completed

    public class Quest
    {
        public event Action<Quest> OnQuestCompleted;

        public readonly string QuestName;
        public int QuestID;
        public QuestStatus QuestStatus;
        public List<QuestComponent> QuestComponents = new List<QuestComponent>();

        private int _componentsCompleted;

        private readonly Dictionary<QuestComponent.QuestComponentType, System.Func<SO_QuestComponent, QuestComponent>>
        _componentFactory = new Dictionary<QuestComponent.QuestComponentType, Func<SO_QuestComponent, QuestComponent>>()
    {
        { QuestComponent.QuestComponentType.EnemyKilled, QC_EnemyKilled.CreateFactory },
        { QuestComponent.QuestComponentType.ItemCollected, QC_ItemCollected.CreateFactory },
        { QuestComponent.QuestComponentType.DialogueQuest, QC_DialogueQuest.CreateFactory }
    };

        public void Activate()
        {
            if (QuestComponents.Count < 1) return;

            QuestComponents[0].EnableComponent();
            QuestStatus = QuestStatus.Active;
        }

        public Quest(string name, int id, List<SO_QuestComponent> questComponents)
        {
            QuestName = name;
            QuestID = id;
            QuestStatus = QuestStatus.Inactive;

            // Don't continue if no components are added to the list
            if (questComponents.Count <= 0)
                return;

            foreach (SO_QuestComponent questComponent in questComponents)
            {
                QuestComponent qcTemp = null;

                if (_componentFactory.ContainsKey(questComponent.questType))
                    qcTemp = _componentFactory[questComponent.questType](questComponent);

                if (qcTemp == null)
                    return;

                QuestComponents.Add(qcTemp);

                // Subscribe to the component so that the Quest knows when the component has been completed.
                qcTemp.OnComponentCompleted += ComponentCompleted;
            }
        }

        ~Quest()
        {
            // Unsubscribe all components from the onComponentComplete event
            for (int i = QuestComponents.Count - 1; i >= 0; i--)
            {
                QuestComponents[i].OnComponentCompleted -= ComponentCompleted;
                QuestComponents[i] = null;
            }
        }

        private void ComponentCompleted(QuestComponent questComponent)
        {
            _componentsCompleted++;

            Debug.unityLogger.Log($"{QuestName}: Component '{questComponent.ComponentName}' was completed");

            if (_componentsCompleted == QuestComponents.Count)
            {
                // Quest has been completed
                QuestStatus = QuestStatus.Completed;
                OnQuestCompleted?.Invoke(this);
            }
            else
            {
                // Enable next component
                if (QuestComponents.Count > _componentsCompleted)
                    QuestComponents[_componentsCompleted].EnableComponent();
            }
        }

        public void Complete()
        {
            // Se já estiver concluída, não faz nada
            if (QuestStatus == QuestStatus.Completed)
                return;

            // Marca como concluída
            QuestStatus = QuestStatus.Completed;

            // Dispara o evento de conclusão
            OnQuestCompleted?.Invoke(this);

            Debug.Log($"🏁 Quest '{QuestName}' foi completada manualmente!");
        }


    }
}