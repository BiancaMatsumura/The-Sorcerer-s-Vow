using UnityEngine;
using TMPro;

public class TitlePulseEffect : MonoBehaviour
{
    [Header("Configurações da Pulsação")]
    public float pulseSpeed = 2f;  
    public float minScale = 0.9f;   
    public float maxScale = 1.2f;  

    private Vector3 originalScale; 
    private TMP_Text titleText;

    void Start()
    {
        // Pega o componente de texto
        titleText = GetComponent<TMP_Text>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        
        float scaleFactor = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
        float currentScale = Mathf.Lerp(minScale, maxScale, scaleFactor); 
        transform.localScale = originalScale * currentScale; 
    }
}