using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    private int itemIndex;
    private Action<int, int> onBuyClicked;

    private int currentQuantity = 1;

    private int minQuantity = 1;
    private int maxQuantity = 99;


    private int unitPrice;

    public void SetData(Sprite sprite, int price, int index, Action<int, int> buyCallback, int minQty, int maxQty)
    {
        itemImage.sprite = sprite;
        unitPrice = price;

        itemIndex = index;
        onBuyClicked = buyCallback;

        minQuantity = Mathf.Max(1, minQty);  // Garantia que nunca seja menor que 1.
        maxQuantity = Mathf.Max(minQuantity, maxQty);  // Garantia que max >= min.

        currentQuantity = minQuantity;

        UpdateQuantityText();
    }


    private void Awake()
    {
        buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(itemIndex, currentQuantity));
        plusButton.onClick.AddListener(IncreaseQuantity);
        minusButton.onClick.AddListener(DecreaseQuantity);

        if (GetComponent<CanvasGroup>() == null)
            gameObject.AddComponent<CanvasGroup>();
    }

    private void IncreaseQuantity()
    {
        if (currentQuantity < maxQuantity)
        {
            currentQuantity++;
            UpdateQuantityText();
        }
    }

    private void DecreaseQuantity()
    {
        if (currentQuantity > minQuantity)
        {
            currentQuantity--;
            UpdateQuantityText();
        }
    }

    private void UpdateQuantityText()
    {
        quantityText.text = $"Qtd: {currentQuantity}\nTotal: ${unitPrice * currentQuantity}";
    }

    public void SetUnavailable()
    {
        GetComponent<CanvasGroup>().alpha = 0.5f;
        GetComponentInChildren<Button>().interactable = false;
    }
}
