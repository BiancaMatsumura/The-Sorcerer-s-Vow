using System.Collections;
using UnityEngine;

public class Item3D : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private float duration = 0.3f;

    [SerializeField]
    private MeshFilter meshFilter; // Novo para definir o modelo 3D

    [SerializeField]
    private MeshRenderer meshRenderer; // Para renderizar o modelo

    private void Start()
    {
        if (InventoryItem != null && meshFilter != null)
        {
            meshFilter.mesh = InventoryItem.ItemMesh; // Usando um Mesh ao invés de Sprite
            meshRenderer.material = InventoryItem.ItemMaterial; // Definindo o material do item
        }
    }

    public void DestroyItem()
    {
        GetComponent<Collider>().enabled = false;
        StartCoroutine(AnimateItemPickup());
    }

    private IEnumerator AnimateItemPickup()
    {
        audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale =
                Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        Destroy(gameObject);
    }
}
