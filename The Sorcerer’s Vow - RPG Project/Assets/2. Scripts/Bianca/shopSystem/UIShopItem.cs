using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private int itemIndex;
    private Action<int> onBuyClicked;

    public void SetData(Sprite sprite, int price, int index, Action<int> buyCallback)
    {
        itemImage.sprite = sprite;
        priceText.text = $"${price}";
        itemIndex = index;
        onBuyClicked = buyCallback;
    }

    private void Awake()
    {
        buyButton.onClick.AddListener(() => onBuyClicked?.Invoke(itemIndex));
        if (GetComponent<CanvasGroup>() == null)
        gameObject.AddComponent<CanvasGroup>();
    }

    public void SetUnavailable()
    {
        GetComponent<CanvasGroup>().alpha = 0.5f; // Deixa mais transparente
        GetComponentInChildren<Button>().interactable = false; // Desativa botão
    }

}
