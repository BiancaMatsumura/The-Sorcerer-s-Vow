using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using Inventory.Model;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private UIInventoryPage inventoryUI;

    [SerializeField] private InventorySO inventoryData;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    [SerializeField]
    private AudioClip dropClip;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField] private UIManager uiManager;

    [SerializeField] private GameObject notificationPanel;


    private void Start()
    {
        PrepareUI();
        PrepareInventoryData();
        notificationPanel.SetActive(false);

    }

    private void HandleInventoryFull()
    {
        Debug.Log("Inventário está cheio!");
        notificationPanel.SetActive(true);
        Invoke(nameof(HideNotification), 2f); // Hide after 2 seconds
    }

    private object HideNotification()
    {
        notificationPanel.SetActive(false);
        return null;
    }

    private void PrepareInventoryData()
    {
        inventoryData.Initialize();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        foreach (InventoryItem item in initialItems)
        {
            if (item.IsEmpty)
                continue;
            inventoryData.AddItem(item);
        }
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllItems();
        foreach (var item in inventoryState)
        {
            if (item.Value.IsEmpty)
                continue;

            inventoryUI.UpdateData(
                item.Key,
                item.Value.item.ItemImage,
                item.Value.quantity,
                item.Value.item.Category
            );
        }

        if (inventoryData.InventoryIsFull())
        {
            HandleInventoryFull();
        }
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI(inventoryData.Size);
        this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        this.inventoryUI.OnSwapItems += HandleSwapItems;
        this.inventoryUI.OnStartDragging += HandleDragging;
        this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    private void HandleItemActionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;

        IItemAction itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {

            inventoryUI.ShowItemAction(itemIndex);
            inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
        }

        IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
        }
    }

    private void DropItem(int itemIndex, int quantity)
    {
        inventoryData.RemoveItem(itemIndex, quantity);
        inventoryUI.ResetSelection();
        audioSource.PlayOneShot(dropClip);
    }

    public void PerformAction(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;

        // Verificar se é um item de ação
        IItemAction itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            // Executar a ação (equipar a arma)
            bool actionSuccess = itemAction.PerformAction(gameObject, inventoryItem.itemState);
            audioSource.PlayOneShot(itemAction.actionSFX);

            // Só remover o item se for destruível E a ação for bem-sucedida
            if (actionSuccess)
            {
                IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
                if (destroyableItem != null)
                {
                    inventoryData.RemoveItem(itemIndex, 1);
                }
            }

            if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                inventoryUI.ResetSelection();
        }
        else
        {
            // Se não for um item de ação, mas for destruível, remova-o
            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }
        }
    }

    private void HandleDragging(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
            return;
        inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
    }

    private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
    {
        inventoryData.SwapItems(itemIndex_1, itemIndex_2);

        // Após o swap, atualize a UI para refletir a mudança de categoria
        Dictionary<int, InventoryItem> currentState = inventoryData.GetCurrentInventoryState();
        UpdateInventoryUI(currentState);
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.IsEmpty)
        {
            inventoryUI.ResetSelection();
            return;
        }

        ItemSO item = inventoryItem.item;
        string description = PrepareDescription(inventoryItem);
        inventoryUI.UpdateDescription(itemIndex, item.ItemImage, item.Name, description);
    }

    private string PrepareDescription(InventoryItem inventoryItem)
    {
        // Segurança inicial
        if (inventoryItem.item == null)
        {
            Debug.LogError("❌ ItemSO NULO no InventoryItem!");
            return "(Item inválido)";
        }

        // Se não houver estado ou parâmetros, retorna só a descrição base
        if (inventoryItem.itemState == null || inventoryItem.itemState.Count == 0)
        {
            Debug.LogWarning($"⚠️ itemState está vazio/nulo no item: {inventoryItem.item.Name}");
            return inventoryItem.item.Description;
        }

        if (inventoryItem.item.DefaultParametersList == null || inventoryItem.item.DefaultParametersList.Count == 0)
        {
            Debug.LogWarning($"⚠️ DefaultParametersList está vazia/nula no item: {inventoryItem.item.Name}");
            return inventoryItem.item.Description;
        }

        // -------------------------
        // Descrição segura
        // -------------------------
        StringBuilder sb = new StringBuilder();
        sb.Append(inventoryItem.item.Description);
        sb.AppendLine();

        int minCount = Mathf.Min(inventoryItem.itemState.Count, inventoryItem.item.DefaultParametersList.Count);

        for (int i = 0; i < minCount; i++)
        {
            var stateParam = inventoryItem.itemState[i];
            var defaultParam = inventoryItem.item.DefaultParametersList[i];

            // Verifica se itemParameter (que deve ser um objeto/referência) é nulo
            if (stateParam.itemParameter == null)
            {
                Debug.LogError($"❌ itemState[{i}].itemParameter está NULO no item: {inventoryItem.item.Name}");
                continue;
            }

            // Verifica se ParameterName é nulo ou vazio
            string paramName = string.IsNullOrEmpty(stateParam.itemParameter.ParameterName)
                ? "Parâmetro Desconhecido"
                : stateParam.itemParameter.ParameterName;

            string stateValue = stateParam.value.ToString();
            string defaultValue = defaultParam.value.ToString();

            // Monta a linha do parâmetro com segurança total
            sb.Append($"{paramName}: {stateValue} / {defaultValue}");
            sb.AppendLine();
        }

        return sb.ToString();
    }



    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiManager.ToggleInventory();
        }
    }

    public void ForceUpdateDescription(ItemSO item, List<ItemParameter> itemState)
    {
        Dictionary<int, InventoryItem> currentState = inventoryData.GetCurrentInventoryState();
        foreach (var kvp in currentState)
        {
            if (kvp.Value.item == item)
            {
                inventoryData.SetItemState(kvp.Key, itemState); // Crie esse método para atualizar o estado no inventário
                HandleDescriptionRequest(kvp.Key); // Atualiza visual
                break;
            }
        }
    }

    public InventorySO InventoryData => inventoryData;

    // ============================================================
    // Adicione este método no seu InventoryController.cs
    // ============================================================

    /// <summary>
    /// Corrige todos os itens existentes no inventário que têm itemParameter nulo.
    /// Use no Inspector: Botão direito no componente > "Fix All Items Parameters"
    /// </summary>
    [ContextMenu("Fix All Items Parameters")]
    private void FixAllItemsParameters()
    {
        int fixedCount = 0;

        for (int i = 0; i < inventoryData.Size; i++)
        {
            var item = inventoryData.GetItemAt(i);

            // Pula slots vazios
            if (item.IsEmpty)
                continue;

            // Verifica se precisa correção
            bool needsFix = false;

            if (item.itemState == null || item.itemState.Count == 0)
            {
                needsFix = true;
            }
            else
            {
                foreach (var param in item.itemState)
                {
                    if (param.itemParameter == null)
                    {
                        needsFix = true;
                        break;
                    }
                }
            }

            // Aplica correção se necessário
            if (needsFix)
            {
                Debug.Log($"🔧 Corrigindo item: {item.item.Name} no slot {i}");

                // Recria os parâmetros usando os defaults
                List<ItemParameter> newState = new List<ItemParameter>();

                if (item.item.DefaultParametersList != null)
                {
                    foreach (var defaultParam in item.item.DefaultParametersList)
                    {
                        newState.Add(new ItemParameter
                        {
                            itemParameter = defaultParam.itemParameter,
                            value = defaultParam.value
                        });
                    }
                }

                inventoryData.SetItemState(i, newState);
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            Debug.Log($"✅ {fixedCount} item(ns) corrigido(s) com sucesso!");
            // Força atualização da UI
            Dictionary<int, InventoryItem> currentState = inventoryData.GetCurrentInventoryState();
            UpdateInventoryUI(currentState);
        }
        else
        {
            Debug.Log("✅ Todos os itens já estão corretos!");
        }
    }

    /// <summary>
    /// Remove TODOS os itens do inventário (útil para testes)
    /// </summary>
    [ContextMenu("Clear All Items")]
    private void ClearAllItems()
    {
        inventoryData.Initialize();
        Dictionary<int, InventoryItem> currentState = inventoryData.GetCurrentInventoryState();
        UpdateInventoryUI(currentState);
        Debug.Log("🗑️ Inventário limpo!");
    }
}
