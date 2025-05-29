using System.Collections;
using UnityEngine;

public class TriggerAtoII : MonoBehaviour
{
    [SerializeField] private GameObject atoIIPanel;
    void Start()
    {
        atoIIPanel.SetActive(false);
        StartCoroutine(ShowPanel());
    }
    
    IEnumerator ShowPanel()
    {
        yield return new WaitForSeconds(5f);
        Time.timeScale = 0f;
        atoIIPanel.SetActive(true);
    
    }


}
