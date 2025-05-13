// DialogueText.cs
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DialogueText : MonoBehaviour
{
    [SerializeField] private float intervalBetweenChars = 0.05f; // Ajuste a velocidade aqui

    private TMP_Text textComponent; // Renomeado para evitar conflito com 'text' obsoleto
    private Coroutine textRevealCoroutine;

    private void Awake() => textComponent = GetComponent<TMP_Text>();

    // Inicia a exibição com animação de digitação
    public Coroutine ShowText(string content)
    {
        // Para corrotina anterior se estiver rodando
        if (textRevealCoroutine != null) StopCoroutine(textRevealCoroutine);

        textComponent.SetText(content);
        textComponent.maxVisibleCharacters = 0; // Começa invisível
        textRevealCoroutine = StartCoroutine(RevealCharts());
        return textRevealCoroutine;
    }

    // Mostra o texto completo imediatamente
    public void ShowImmediate(string content)
    {
        // Para corrotina anterior se estiver rodando
        if (textRevealCoroutine != null) StopCoroutine(textRevealCoroutine);

        textComponent.SetText(content);
        textComponent.maxVisibleCharacters = content.Length; // Mostra tudo
    }

    // Esconde o texto completamente
    public void HideText()
    {
        // Para corrotina anterior se estiver rodando
        if (textRevealCoroutine != null) StopCoroutine(textRevealCoroutine);

        textComponent.SetText("");
        textComponent.maxVisibleCharacters = 0;
    }

    // Pula a animação atual, mostrando todo o texto
    public void SkipAnimation()
    {
        // Para corrotina anterior se estiver rodando
        if (textRevealCoroutine != null) StopCoroutine(textRevealCoroutine);

        // Garante que todo o texto seja visível
        if (textComponent != null && textComponent.textInfo != null) // Checagem extra
        {
            textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        }
        // Importante: A lógica que atualiza o ESTADO do DialogueManager
        // (de typing para waiting) precisa acontecer DEPOIS que a corrotina terminar
        // ou ser forçada aqui se a corrotina for parada abruptamente.
        // No nosso caso, a corrotina ShowTextAndAdvance no Manager cuida disso.
    }

    // Corrotina interna para revelar caracteres
    private IEnumerator RevealCharts()
    {
        // Previne erros se o texto ou textInfo não estiverem prontos
        yield return null; // Espera um frame para garantir inicialização do TextInfo
        if (textComponent == null || textComponent.textInfo == null) yield break;

        int totalVisibleCharacters = textComponent.textInfo.characterCount;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            textComponent.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(intervalBetweenChars);
        }
        // Garante que o último caractere seja exibido mesmo se a contagem for estranha
        textComponent.maxVisibleCharacters = totalVisibleCharacters;
        textRevealCoroutine = null; // Limpa referência da corrotina ao terminar
    }
}
