using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Model;

public class UIInventoryPage : MonoBehaviour
{
    [SerializeField]
    private UIInventoryItem inventoryItemUIPrefab;

    [SerializeField]
    private RectTransform contentPanel;

    [SerializeField]
    private UIInventoryDescription inventoryItemUIDescription;

    [SerializeField]
    private MouseFollower mouseFollower;
    
    [SerializeField]
    private ItemActionPanel itemActionPanel;

    [Header("Category Tabs")]
    [SerializeField]
    private Transform tabsContainer;
    
    [SerializeField]
    private GameObject tabButtonPrefab;
    
    [SerializeField]
    private Color selectedTabColor = Color.white;
    
    [SerializeField]
    private Color unselectedTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private Dictionary<ItemCategory, Button> categoryTabs = new Dictionary<ItemCategory, Button>();
    private ItemCategory currentCategory = ItemCategory.Default;
    
    private List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();
    private Dictionary<ItemCategory, List<UIInventoryItem>> categorizedItems = new Dictionary<ItemCategory, List<UIInventoryItem>>();
    private Dictionary<int, ItemCategory> slotCategories = new Dictionary<int, ItemCategory>();

    public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging;
    public event Action<int, int> OnSwapItems;
    public event Action<ItemCategory> OnCategoryChanged;

    private int currentlyDraggedItemIndex = -1;

    private void Awake()
    {
        inventoryItemUIDescription.ResetDescription();
    }

    public void InitializeInventoryUI(int inventorySize)
    {
        // Initialize the category tabs first
        InitializeCategoryTabs();
        
        // Then create inventory slots
        for (int i = 0; i < inventorySize; i++)
        {
            UIInventoryItem uiItem = Instantiate(inventoryItemUIPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel);
            listOfUIItems.Add(uiItem);
            uiItem.OnItemClicked += HandleItemSelection;
            uiItem.OnItemBeginDrag += HandleBeginDrag;
            uiItem.OnItemDroppedOn += HandleSwap;
            uiItem.OnItemEndDrag += HandleEndDrag;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
        }
        
        // Initialize empty categorized lists
        foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory)))
        {
            categorizedItems[category] = new List<UIInventoryItem>();
        }
        
        // Start with the Default category selected
        SelectCategory(ItemCategory.Default);
    }
    
    private void InitializeCategoryTabs()
    {
        // Skip if tabsContainer is not assigned
        if (tabsContainer == null || tabButtonPrefab == null)
            return;
            
        // Get all categories from the enum
        ItemCategory[] categories = (ItemCategory[])Enum.GetValues(typeof(ItemCategory));
        
        foreach (ItemCategory category in categories)
        {
            // Create a tab button for each category
            GameObject tabObj = Instantiate(tabButtonPrefab, tabsContainer);
            Button tabButton = tabObj.GetComponent<Button>();
            TextMeshProUGUI tabText = tabObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (tabText != null)
            {
                tabText.text = category.ToString();
            }
            
            // Store reference to the tab button
            categoryTabs[category] = tabButton;
            
            // Add click listener
            ItemCategory capturedCategory = category; // Capture for lambda
            tabButton.onClick.AddListener(() => SelectCategory(capturedCategory));
        }
    }
    
   public void SelectCategory(ItemCategory category)
    {
        currentCategory = category;
        
        // Update tab button visuals
        foreach (var tab in categoryTabs)
        {
            Image tabImage = tab.Value.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = (tab.Key == category) ? selectedTabColor : unselectedTabColor;
            }
        }
        
        // Update UI items visibility
        UpdateItemsVisibility();
        
        // Reset selection when changing categories
        ResetSelection();
        
        // Force layout refresh
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
        
        // Notify any listeners about category change
        OnCategoryChanged?.Invoke(category);
    }
    private void UpdateItemsVisibility()
    {
        // NÃO usaremos SetActive(false) para itens que não estão na categoria atual
        // Em vez disso, vamos ajustar a visibilidade sem desativar o GameObject completamente
        
        if (currentCategory == ItemCategory.Default)
        {
            // No modo default, mostrar todos os slots (ocupados ou não)
            foreach (UIInventoryItem item in listOfUIItems)
            {
                // Não desative o objeto, apenas ajuste a visibilidade se necessário
                item.gameObject.SetActive(true);
            }
            return;
        }
        
        // Para outras categorias, contamos quantos slots pertencem a essa categoria
        int visibleCount = 0;
        
        // Primeiro, marque todos como invisíveis
        foreach (UIInventoryItem item in listOfUIItems)
        {
            item.gameObject.SetActive(false);
        }
        
        // Depois, ative apenas os slots que contêm itens da categoria selecionada
        // E os reposicione no início do conteúdo
        for (int i = 0; i < listOfUIItems.Count; i++)
        {
            if (slotCategories.TryGetValue(i, out ItemCategory category) && 
                category == currentCategory)
            {
                listOfUIItems[i].gameObject.SetActive(true);
                // Reposicione o item na hierarquia para ficar na ordem correta visualmente
                listOfUIItems[i].transform.SetSiblingIndex(visibleCount);
                visibleCount++;
            }
        }
        
        // Force layout refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
    }
    public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity, ItemCategory category)
    {
        if (listOfUIItems.Count > itemIndex)
        {
            listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
            
            // Store the category of this item for filtering
            if (itemImage != null && itemQuantity > 0)
            {
                Debug.Log($"Slot {itemIndex}: Atribuindo categoria {category}");
                slotCategories[itemIndex] = category;
                
                // Certifique-se de que a lista categorizada existe
                if (!categorizedItems.ContainsKey(category))
                {
                    categorizedItems[category] = new List<UIInventoryItem>();
                }
                
                // Add to categorized list if not already there
                if (!categorizedItems[category].Contains(listOfUIItems[itemIndex]))
                {
                    categorizedItems[category].Add(listOfUIItems[itemIndex]);
                }
            }
            else
            {
                // Remove from categorized list and slot categories if empty
                if (slotCategories.ContainsKey(itemIndex))
                {
                    ItemCategory oldCategory = slotCategories[itemIndex];
                    if (categorizedItems.ContainsKey(oldCategory))
                    {
                        categorizedItems[oldCategory].Remove(listOfUIItems[itemIndex]);
                    }
                    slotCategories.Remove(itemIndex);
                }
            }
            
            // Update visibility based on current category
            UpdateItemsVisibility();
        }
    }
    
    // Overload for backward compatibility
    public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
    {
        // For items with unknown category, default to Default category
        UpdateData(itemIndex, itemImage, itemQuantity, ItemCategory.Default);
    }

    private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
    {
        int index = listOfUIItems.IndexOf(inventoryItemUI);
        if (index == -1)
        {
            return;
        }
        OnItemActionRequested?.Invoke(index);
    }

    private void HandleEndDrag(UIInventoryItem inventoryItemUI)
    {
        ResetDraggedItem();
    }

    private void HandleSwap(UIInventoryItem inventoryItemUI)
{
    // Se estamos em uma categoria específica que não é Default,
    // só permitimos trocar itens dentro dessa categoria
    int index = listOfUIItems.IndexOf(inventoryItemUI);
    if (index == -1)
    {
        return;
    }
    
    // Verificamos se o item arrastado e o item alvo pertencem à categoria atual
    // ou se estamos na categoria Default
    bool canSwap = currentCategory == ItemCategory.Default;
    
    if (!canSwap)
    {
        // Se não for Default, verificamos se ambos itens pertencem à categoria atual
        bool sourceIsInCategory = slotCategories.TryGetValue(currentlyDraggedItemIndex, out ItemCategory sourceCategory) &&
                                 sourceCategory == currentCategory;
        
        bool targetIsInCategory = slotCategories.TryGetValue(index, out ItemCategory targetCategory) &&
                                 targetCategory == currentCategory;
        
        // Se o alvo está vazio, o swap ainda é válido
        bool targetIsEmpty = !slotCategories.ContainsKey(index);
                                 
        canSwap = sourceIsInCategory && (targetIsInCategory || targetIsEmpty);
    }
    
    if (canSwap)
    {
        OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
        HandleItemSelection(inventoryItemUI);
    }
    else
    {
        // Reseta o drag se não for possível trocar
        ResetDraggedItem();
    }
}

    private void ResetDraggedItem()
    {
        mouseFollower.Toggle(false);
        currentlyDraggedItemIndex = -1;
    }

    private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
    {
        int index = listOfUIItems.IndexOf(inventoryItemUI);
        if (index == -1)
            return;
        
        // Verificamos se o item pertence à categoria atual
        bool canDrag = currentCategory == ItemCategory.Default;
        
        if (!canDrag)
        {
            canDrag = slotCategories.TryGetValue(index, out ItemCategory category) &&
                    category == currentCategory;
        }
        
        if (canDrag)
        {
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }
    }

    private void HandleItemSelection(UIInventoryItem inventoryItemUI)
    {
        int index = listOfUIItems.IndexOf(inventoryItemUI);
        if (index == -1)
            return;
        OnDescriptionRequested?.Invoke(index);
    }

    public void CreateDraggedItem(Sprite sprite, int quantity)
    {
        mouseFollower.Toggle(true);
        mouseFollower.SetData(sprite, quantity);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        ResetSelection();
    }

    public void ResetSelection()
    {
        inventoryItemUIDescription.ResetDescription();
        DeselectAllItems();
    }

    public void AddAction(string actionName, Action performAction)
    {
        itemActionPanel.AddButon(actionName, performAction);
    }

    public void ShowItemAction(int itemIndex)
    {
        itemActionPanel.Toggle(true);
        itemActionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
    }

    private void DeselectAllItems()
    {
        foreach (UIInventoryItem item in listOfUIItems)
        {
            item.Deselect();
        }
        itemActionPanel.Toggle(false);
    }

    public void Hide()
    {
        itemActionPanel.Toggle(false);
        gameObject.SetActive(false);
        ResetDraggedItem();
        DeselectAllItems();
    }

    internal void UpdateDescription(int itemIndex, Sprite itemImage, string name, string description)
    {
        inventoryItemUIDescription.SetDescription(itemImage, name, description);
        DeselectAllItems();
        listOfUIItems[itemIndex].Select();
    }

    internal void ResetAllItems()
{
    foreach (var item in listOfUIItems)
    {
        item.ResetData();
        item.Deselect();
    }
    
    // Limpe os dados de categorização
    slotCategories.Clear();
    foreach (var category in categorizedItems.Keys.ToList())
    {
        categorizedItems[category].Clear();
    }
    
    // Reset para categoria padrão
    SelectCategory(ItemCategory.Default);
}
}