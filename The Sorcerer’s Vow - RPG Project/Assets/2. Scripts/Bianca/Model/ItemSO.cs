using UnityEngine;

[CreateAssetMenu]
public class ItemSO : ScriptableObject
{   
    [field: SerializeField]
        public bool IsStackable { get; set; }

        public int ID => GetInstanceID();

        [field: SerializeField]
        public int MaxStackSize { get; set; } = 1;

        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }

        [field: SerializeField]
        public Sprite ItemImage { get; set; }

        [field: SerializeField]
        public Mesh ItemMesh { get; set; }
        
        [field: SerializeField]
        public Material ItemMaterial { get; set; }
}

