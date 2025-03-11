using System;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

namespace Inventory.Model
{
    public enum ItemCategory
{
    Default,
    Weapon,
    Armor,
    Consumable,
    Material
}
    public abstract class ItemSO : ScriptableObject
{     
    [field: SerializeField]
    public string Name { get; set; }

    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }

    [SerializeField]
    private ItemCategory category;

    public ItemCategory Category => category;

    [field: SerializeField]
    public bool IsStackable { get; set; }

    [field: SerializeField]
    public int MaxStackSize { get; set; } = 1;

    public int ID => GetInstanceID();

    [field: SerializeField]
    public Sprite ItemImage { get; set; }

    [field: SerializeField]
    public Mesh ItemMesh { get; set; }
        
    [field: SerializeField]
    public Material ItemMaterial { get; set; }

    [field: SerializeField]
    public List<ItemParameter> DefaultParametersList { get; set; }

}

    [Serializable]
    public struct ItemParameter : IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other)
        {
            return other.itemParameter == itemParameter;
        }
    }
}


