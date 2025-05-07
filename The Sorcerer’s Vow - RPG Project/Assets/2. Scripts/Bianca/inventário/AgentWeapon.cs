using Inventory.Model;
using System.Collections.Generic;
using UnityEngine;

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
    Animator anim;

    private GameObject equippedWeaponObject;

    private float danoAtual;
    private float durabilidadeAtual;

    public float CurrentDano => danoAtual;
    public float CurrentDurabilidade => durabilidadeAtual;



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

        // Reduz 1 ponto de durabilidade
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

        // Atualiza visual da descrição no inventário (se ele estiver aberto)
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

        return 0; // Sem dano
    }





    public void SetIngredient(EquippableItemSO ingredientItemSO, List<ItemParameter> itemState)
    {
        if (equippedIngredient != null)
            inventoryData.AddItem(equippedIngredient, 1, ingredientCurrentState);

        equippedIngredient = ingredientItemSO;
        ingredientCurrentState = new List<ItemParameter>(itemState);
        ModifyParameters(ingredientCurrentState);
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
    }
}
