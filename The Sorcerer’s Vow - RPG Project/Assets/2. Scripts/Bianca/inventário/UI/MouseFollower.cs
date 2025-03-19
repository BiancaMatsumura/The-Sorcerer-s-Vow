using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    
    [SerializeField] private Canvas canvas;

    [SerializeField]
    private UIInventoryItem item;

    private void Awake()
{
    canvas = Object.FindFirstObjectByType<Canvas>();

    if (canvas == null)
    {
        Debug.LogError("Nenhum Canvas encontrado na cena! Certifique-se de que há um Canvas presente.");
    }

    
}


    public void SetData(Sprite sprite, int quantity)
    {
        item.SetData(sprite, quantity);
    }
    void Update()
    {
        if (canvas == null)
        {
            Debug.LogWarning("Canvas foi destruído!");
            return;
        }

        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            Input.mousePosition,
            canvas.worldCamera,
            out position
        );
        
        transform.position = canvas.transform.TransformPoint(position);
    }
    public void Toggle(bool val)
    {
        Debug.Log($"Item toggled {val}");
        gameObject.SetActive(val);
    }
}
