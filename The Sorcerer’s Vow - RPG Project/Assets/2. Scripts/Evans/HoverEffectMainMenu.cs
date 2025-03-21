using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverEfeito : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image botaoImage;

    [Header("Configurações de Cores")]
    public Color corNormal = Color.white;
    public Color corHover = new Color(0.6f, 0.1f, 0.1f, 1f);

    [Header("Velocidade da Transição")]
    [Range(1f, 20f)]
    public float velocidadeTransicao = 5f;

    private bool mouseEmCima = false;

    void Start()
    {
        botaoImage = GetComponent<Image>();
        corNormal = botaoImage.color;
    }

    void Update()
    {
        
        botaoImage.color = Color.Lerp(botaoImage.color, mouseEmCima ? corHover : corNormal, Time.deltaTime * velocidadeTransicao);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEmCima = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEmCima = false;
    }
}
