using UnityEngine;

namespace QuestSystem
{
    public class QC_ItemCollected : QuestComponent
    {
        private int _itemID;
        private int _itemCount;
        private int _itemAmountNeeded;

        public QC_ItemCollected(string name, string description, int itemID, int itemAmountNeeded) 
            : base(name, description)
        {
            _itemID = itemID;
            _itemAmountNeeded = itemAmountNeeded;
            _itemCount = 0;
            ComponentType = QuestComponentType.ItemCollected;
        }

        public static QuestComponent CreateFactory(SO_QuestComponent so_questComponent)
        {
            SO_QC_ItemCollected localQuestComponent = (SO_QC_ItemCollected)so_questComponent;

            return new QC_ItemCollected(
                localQuestComponent.componentName,
                localQuestComponent.description,
                localQuestComponent.itemID,
                localQuestComponent.itemAmountNeeded);
        }

        public override void EnableComponent()
        {
            base.EnableComponent();
            QuestEvents.OnItemCollected += ItemCollected;
        }

        public override void MarkCompleted()
        {
            base.MarkCompleted();
            QuestEvents.OnItemCollected -= ItemCollected;
        }

        private void ItemCollected(int itemID)
        {
            if (_itemID != itemID)
                return;

            _itemCount++;

            Debug.unityLogger.Log($"{ComponentName}: Item Type {_itemID} was collected {_itemCount}/{_itemAmountNeeded}");

            if (_itemCount < _itemAmountNeeded)
                return;

            // Marca o componente como completo
            MarkCompleted();
            TriggerComponentCompleted(this);
        }
    }
}
