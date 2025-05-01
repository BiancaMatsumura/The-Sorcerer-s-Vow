using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
{
    public string ActionName => "Equip";

    [field: SerializeField] public AudioClip actionSFX { get; private set; }
    [field: SerializeField] public ItemCategory slotType { get; private set; } = ItemCategory.Armas;

    public GameObject Prefabs;
    public bool equip; 

    public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
    {
        AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
        if (weaponSystem != null)
        {
            if (slotType == ItemCategory.Armas)
                weaponSystem.SetWeapon(this, itemState ?? DefaultParametersList, Prefabs);
            else
                weaponSystem.SetIngredient(this, itemState ?? DefaultParametersList);

            return true;
        }

        return false;
    }
}

}


