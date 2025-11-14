using Inventory.Model;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class AgentWeapon : MonoBehaviour
{
    private EquippableItemSO weapon;
    private EquippableItemSO equippedIngredient;

    [SerializeField]
    private InventorySO inventoryData;

    [SerializeField]
    private List<ItemParameter> parametersToModify, itemCurrentState, ingredientCurrentState;

    public EquippableItemSO CurrentWeapon => weapon;
    public EquippableItemSO CurrentIngredient => equippedIngredient;
    public Transform handTransform;
    

    private GameObject equippedWeaponObject;
    private float danoAtual;
    private float durabilidadeAtual;

    public float CurrentDano => danoAtual;
    public float CurrentDurabilidade => durabilidadeAtual;

    // Novo booleano para verificar se há uma arma equipada
    public bool HasWeapon { get; private set; } = false;

    // Referência ao trigger de dano
    private WeaponDamageTrigger weaponTrigger;
    //Feedback
    private Color CorDaUi;
    public Image[] Sprite;
    public Animation[] Uianimation;
    public AudioSource[] audioClips;
    Animator anim;
    public Image Item_Image;
    public Sprite DefaultSprite;
    public GameObject MATERIAL;
    public Material Magic;

    private void Awake()
    {
        weaponTrigger = GetComponentInChildren<WeaponDamageTrigger>();
      
        
    }

    public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState)
    {
        anim = GetComponent<Animator>();

        anim.SetInteger("Weapon", weaponItemSO.animIndex);
        anim.Play(weaponItemSO.animName);

        

        if (equippedWeaponObject != null)
            Destroy(equippedWeaponObject);

        if (weaponItemSO.WorldPrefab != null)
            equippedWeaponObject = Instantiate(weaponItemSO.WorldPrefab, handTransform);

        if (weapon != null)
            inventoryData.AddItem(weapon, 1, itemCurrentState);

        weapon = weaponItemSO;
        itemCurrentState = new List<ItemParameter>(itemState);

        ApplyWeaponStats(itemCurrentState);

        HasWeapon = true;

        Item_Image.sprite = weaponItemSO.ItemImage;

        if (HasWeapon == false) 
        {
            Item_Image.sprite = DefaultSprite;
        }
    }

    private void ApplyWeaponStats(List<ItemParameter> parameters)
    {
        foreach (var param in parameters)
        {
            switch (param.itemParameter.ParameterName.ToLower())
            {
                case "dano":
                    danoAtual = param.value;
                    break;
                case "durabilidade":
                    durabilidadeAtual = param.value;
                    break;
            }
        }

        Debug.Log($"Dano: {danoAtual} | Durabilidade: {durabilidadeAtual}");
    }

    public void UseWeapon()
    {
        if (weapon == null || itemCurrentState == null) return;

        for (int i = 0; i < itemCurrentState.Count; i++)
        {
            if (itemCurrentState[i].itemParameter.ParameterName == "Durabilidade")
            {
                float newValue = Mathf.Max(0, itemCurrentState[i].value - 1);
                itemCurrentState[i] = new ItemParameter
                {
                    itemParameter = itemCurrentState[i].itemParameter,
                    value = newValue
                };
                break;
            }
        }

        Object.FindFirstObjectByType<InventoryController>()?.ForceUpdateDescription(weapon, itemCurrentState);
    }

    public float GetWeaponDamage()
    {
        if (itemCurrentState == null) return 0;

        foreach (var param in itemCurrentState)
        {
            if (param.itemParameter.ParameterName == "Dano")
                return param.value;
        }

        return 0;
    }

    public void SetIngredient(EquippableItemSO ingredientItemSO, List<ItemParameter> itemState)
    {
        anim = GetComponent<Animator>();
        if (equippedIngredient != null)
            inventoryData.AddItem(equippedIngredient, 1, ingredientCurrentState);
        
        equippedIngredient = ingredientItemSO;
        ingredientCurrentState = new List<ItemParameter>(itemState);
        ModifyParameters(ingredientCurrentState);
        anim.Play("Essenciaa");
        Sprite[0].color = equippedIngredient.Cor;
        Sprite[1].color = equippedIngredient.Cor;
        Uianimation[0].Play("Craft");
        audioClips[0].Play();  
        Magic.SetColor("_Color", AjustarHDR(equippedIngredient.Cor));    
        MagicVFX();
    }

    private void ModifyParameters(List<ItemParameter> parameters)
    {
        foreach (var parameter in parametersToModify)
        {
            if (parameters.Contains(parameter))
            {
                int index = parameters.IndexOf(parameter);
                float newValue = parameters[index].value + parameter.value;
                parameters[index] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
        }
    }

    public void UnequipIngredient()
    {
        equippedIngredient = null;
        ingredientCurrentState = null;
    }

    public void UnequipWeapon()
    {
        weapon = null;
        itemCurrentState = null;
        HasWeapon = false;
    }

    // Chamado no início da animação de ataque
    public void ResetWeaponHitList()
    {
        weaponTrigger?.ResetHits();
    }


    public void MagicVFX() 
    {
        Magic.SetFloat("_EmissivePower", 10f);
        Invoke("DeleteMagic", 3f);
    }
    public void DeleteMagic()
    {
        Magic.SetFloat("_EmissivePower", 0f);
        Magic.SetColor("Color", default);
    }
    Color AjustarHDR(Color corBase)
    {
        Color corHDR = corBase * 10000f ; // aumenta intensidade (HDR)
        corHDR.a = 1f;               // força alpha máximo
        return corHDR;
    }
}
