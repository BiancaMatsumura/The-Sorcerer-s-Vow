using UnityEngine;
using Inventory.Model;
using System.Collections.Generic;

public class CraftingTrigger : MonoBehaviour
{
    [SerializeField] private Frutos frutoAtual;
    [SerializeField] private List<CraftingRecipeSO> recipes;

    [SerializeField]
    private GameObject interactObject;

    [SerializeField] 
    private KeyCode interactionKey = KeyCode.E;

    [SerializeField] private AudioSource audioPadrao;
    private Transform mainCamera;
    private bool isIn = false;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (interactObject.activeSelf)
        {
            interactObject.transform.LookAt(mainCamera);
            interactObject.transform.Rotate(0f, 180f, 0f);
        }

        if (Input.GetKeyDown(interactionKey) && isIn)
        {
            TryCraft();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jogador entrou no trigger!");
            isIn = true;
            interactObject.SetActive(true);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isIn = false;
            interactObject.SetActive(false);
        }
    }

    private void TryCraft()
    {
         Debug.Log("Tentando craftar...");
        var player = GameObject.FindGameObjectWithTag("Player"); // Aqui buscamos o jogador.
        var weaponSystem = player.GetComponent<AgentWeapon>();
        if (weaponSystem == null || weaponSystem.CurrentIngredient == null) return;

        var equippedItem = weaponSystem.CurrentIngredient;

        Debug.Log($"Item equipado: {equippedItem.Name} (ID: {equippedItem.ID})");

        foreach (var recipe in recipes)
        {
            if (recipe.fruto != frutoAtual)
                continue;

            if (recipe.ingredients.Count != 1)
                continue;

            var requiredItem = recipe.ingredients[0].item;

            Debug.Log($"Verificando receita com item: {requiredItem.Name} (ID: {requiredItem.ID})");

            if (recipe.resultItem.WorldPrefab != null)
            {
                var instance = Instantiate(recipe.resultItem.WorldPrefab, transform.position + Vector3.up, Quaternion.identity);
                var item3DComponent = instance.GetComponent<Item3D>();
                item3DComponent.audioSource = audioPadrao;

                // Remove o item do slot de ingrediente
                weaponSystem.UnequipIngredient();

                Debug.Log($"Criado: {recipe.resultItem.Name}");
            }
            else
            {
                Debug.LogWarning("Prefab do item de resultado não definido!");
            }
        }

    }
}
