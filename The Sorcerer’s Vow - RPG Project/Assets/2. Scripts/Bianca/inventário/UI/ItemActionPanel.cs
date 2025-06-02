using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemActionPanel : MonoBehaviour
{
    [SerializeField]
        private GameObject buttonPrefab;

        public void AddButon(string name, Action onClickAction)
        {
            GameObject button = Instantiate(buttonPrefab, transform);
            button.GetComponent<Button>().onClick.AddListener(() => onClickAction());
            button.GetComponentInChildren<TMPro.TMP_Text>().text = name;
        }

        public void Toggle(bool val)
        {
            if (this == null) return; // Evita erro se o painel foi destruído
            if (val == true)
                RemoveOldButtons();
            if (gameObject != null)
                gameObject.SetActive(val);
        }

        public void RemoveOldButtons()
        {
            foreach (Transform transformChildObjects in transform)
            {
                Destroy(transformChildObjects.gameObject);
            }
        }
}

public class SomeOtherClass : MonoBehaviour
{
    [SerializeField]
    private ItemActionPanel itemActionPanel;

    private void SomeMethod()
    {
        // Some code...

        if (itemActionPanel != null)
            itemActionPanel.Toggle(false);

        // Some more code...
    }
}
