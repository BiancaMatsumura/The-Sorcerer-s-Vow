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
    public void SetWeapon(EquippableItemSO weaponItemSO, List<ItemParameter> itemState, GameObject Prefab)
    {   

        anim = GetComponent<Animator>();
        anim.SetBool("Sword", true);
        anim.Play("swordStart");//ainda é necessario uma logica pra indentificar qual é a arma e sua animação
        Instantiate(Prefab, handTransform);
        
        if (weapon != null)
            inventoryData.AddItem(weapon, 1, itemCurrentState);

        weapon = weaponItemSO;
        itemCurrentState = new List<ItemParameter>(itemState);
        ModifyParameters(itemCurrentState);
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
