using UnityEngine;
using Inventory.Model;
using System.Collections.Generic;
using QuestSystem;

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
    public Animator PlayerAnimator;
    public Animation UIanimator;
    private AudioSource CraftSound;

    void Start()
    {
        mainCamera = Camera.main.transform;
        CraftSound = GetComponent<AudioSource>();

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
        if (other.CompareTag("Player"))
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

            // ✅ Aqui garantimos que só vai criar se o item equipado for o mesmo da receita.
            if (requiredItem.ID != equippedItem.ID)
            {
                Debug.Log("Ingrediente equipado não corresponde ao necessário para esta receita.");
                continue;
            }

            if (recipe.resultItem.WorldPrefab != null)
            {
                var instance = Instantiate(recipe.resultItem.WorldPrefab, transform.position + Vector3.up, Quaternion.identity);
                var item3DComponent = instance.GetComponent<Item3D>();
                item3DComponent.audioSource = audioPadrao;

                // Remove o item do slot de ingrediente
                weaponSystem.UnequipIngredient();

                Debug.Log($"Criado: {recipe.resultItem.Name}");
                if (PlayerAnimator != null)
                {
                    PlayerAnimator.Play("CraftAnimation");
                    UIanimator.Play("FillUIMagic");
                    CraftSound.Play();
                }
                

                // ✅ Opcional: se quiser que só crafte uma vez e pare o loop:
                break;
            }
            else
            {
                Debug.LogWarning("Prefab do item de resultado não definido!");
            }
        }
    }



}
