using UnityEngine;
using TMPro;

public class HeartbeatEffect : MonoBehaviour
{
    [Header("Configurações do Batimento")]
    public float heartbeatSpeed = 1.5f;
    public float minScale = 0.9f;
    public float maxScale = 1.3f;
    public AnimationCurve pulseCurve;

    [Header("Efeito de Brilho")]
    public Color baseColor = Color.white;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 2f;

    private TMP_Text titleText;
    private Vector3 originalScale;

    void Start()
    {
        titleText = GetComponent<TMP_Text>();

        if (titleText == null)
        {
            Debug.LogError("❌ ERRO: Nenhum TMP_Text encontrado no objeto " + gameObject.name);
            return;
        }

        originalScale = transform.localScale;
    }

    void OnEnable()
    {
      
        if (titleText != null)
        {
            transform.localScale = originalScale;
            titleText.color = baseColor;
        }
    }

    void Update()
    {
        if (titleText == null) return;

        float pulseProgress = Mathf.PingPong(Time.time * heartbeatSpeed, 1f);
        float curveValue = pulseCurve.Evaluate(pulseProgress);

        
        float currentScale = Mathf.Lerp(minScale, maxScale, curveValue);
        transform.localScale = originalScale * currentScale;

       
        Color currentColor = Color.Lerp(baseColor, glowColor, curveValue * glowIntensity);
        titleText.color = currentColor;
    }
}
