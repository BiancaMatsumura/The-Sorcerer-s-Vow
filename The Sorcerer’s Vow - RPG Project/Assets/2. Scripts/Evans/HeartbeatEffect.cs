using UnityEngine;
using TMPro;

public class HeartbeatEffect : MonoBehaviour
{
    [Header("Configurações do Batimento")]
    public float heartbeatSpeed = 1.5f; // Velocidade do batimento
    public float minScale = 0.9f;       // Escala mínima
    public float maxScale = 1.3f;       // Escala máxima (mais exagerada)
    public AnimationCurve pulseCurve;  // Curva personalizada para o batimento

    [Header("Efeito de Brilho")]
    public Color baseColor = Color.white;   // Cor base do texto
    public Color glowColor = Color.yellow;  // Cor do brilho
    public float glowIntensity = 2f;        // Intensidade do brilho

    private TMP_Text titleText;
    private Vector3 originalScale;

    void Start()
    {
        titleText = GetComponent<TMP_Text>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Calcula o progresso do batimento usando tempo e curva
        float pulseProgress = Mathf.PingPong(Time.time * heartbeatSpeed, 1f);
        float curveValue = pulseCurve.Evaluate(pulseProgress);

        // Aplica a escala
        float currentScale = Mathf.Lerp(minScale, maxScale, curveValue);
        transform.localScale = originalScale * currentScale;

        // Aplica o brilho (interpola entre a cor base e a cor de brilho)
        Color currentColor = Color.Lerp(baseColor, glowColor, curveValue * glowIntensity);
        titleText.color = currentColor;
    }
}