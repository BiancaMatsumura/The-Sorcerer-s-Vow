using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DialogueText : MonoBehaviour
{
    [SerializeField] private float intervalBetweenChars = 0.1f;

    private TMP_Text text;

    private void Awake() => text = GetComponent<TMP_Text>();

    public IEnumerator ShowText(string content)
    {
        text.maxVisibleCharacters = 0;
        text.SetText(content);
        yield return RevealCharts();
    }

    public void HideText() 
    {
        text.SetText("");
        text.maxVisibleCharacters = 0;
    }

    public void SkipAnimation() => text.maxVisibleCharacters = text.textInfo.characterCount;

    private IEnumerator RevealCharts()
    {
        while (text.maxVisibleCharacters <= text.textInfo.characterCount)
        {
            yield return new WaitForSeconds(intervalBetweenChars);
            text.maxVisibleCharacters++;
        }
    }
}
