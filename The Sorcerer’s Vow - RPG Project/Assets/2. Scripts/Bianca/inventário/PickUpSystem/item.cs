using System.Collections;
using UnityEngine;
using Inventory.Model;

public class Item3D : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    public AudioSource audioSource;

    [SerializeField]
    private float duration = 0.3f;

    [SerializeField]
    private QuestData linkedQuest;

    private InventoryController inventoryController;

    [SerializeField] private GameObject interectionUI;
    private Transform mainCamera;

    private Animation anim;

    public ParticleSystem ParticleDesable;

    private bool isCollected = false;

    private void Awake()
    {   
        mainCamera = Camera.main.transform;
        anim = GetComponent<Animation>();
        inventoryController = Object.FindAnyObjectByType<InventoryController>();
    }

    private void Update()
    {
        if (interectionUI != null)
        {
            if (mainCamera != null)
            {
                interectionUI.transform.LookAt(mainCamera);
                interectionUI.transform.Rotate(0f, 180f, 0f);
            }
        }
    }

    public void DestroyItem()
    {
        if (isCollected)
        {
            Debug.Log("[Item3D] Item já foi coletado!");
            return;
        }

        if (!CanPickup())
        {
            Debug.Log("[Item3D] ❌ Quest ainda não pode ser completada (dependências pendentes).");
            return;
        }

        if (inventoryController == null)
        {
            Debug.LogError("[Item3D] InventoryController não encontrado!");
            return;
        }

        if (inventoryController.InventoryData.CanAddItem(InventoryItem, Quantity))
        {
            isCollected = true;
            GetComponent<Collider>().enabled = false;

            Debug.Log($"[Item3D] ✅ Coletando item: {InventoryItem.Name}");

            if(ParticleDesable!= null)
            {
                ParticleDesable.Clear();
            }   
            // ✅ ADICIONA ITEM AO INVENTÁRIO IMEDIATAMENTE
            inventoryController.InventoryData.AddItem(InventoryItem, Quantity);
            Debug.Log($"[Item3D] Item '{InventoryItem.Name}' adicionado ao inventário");

            StartCoroutine(AnimateItemPickup());
        }
        else
        {
            Debug.Log("[Item3D] ❌ Não é possível coletar: inventário cheio.");
        }
    }

    private IEnumerator AnimateItemPickup()
    {
        // Toca som de coleta
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Para animação idle se houver
        if (anim != null)
        {
            anim.Stop();
        }

        // Animação de encolher
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }

        // ✅ NOTIFICA O SISTEMA DE QUEST
        if (linkedQuest != null)
        {
            var questSystem = Object.FindAnyObjectByType<QuestSystem>();
            if (questSystem != null)
            {
                Debug.Log($"[Item3D] 🔔 Notificando QuestSystem: Item coletado para quest '{linkedQuest.questName}'");
                questSystem.NotifyItemCollected(linkedQuest);
            }
            else
            {
                Debug.LogWarning("[Item3D] QuestSystem não encontrado na cena!");
            }
        }
        else
        {
            Debug.Log("[Item3D] Item não está vinculado a nenhuma quest");
        }

        // Aguarda um pouco antes de destruir
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"[Item3D] Destruindo GameObject do item '{InventoryItem.Name}'");
        Destroy(gameObject);
    }

    public bool CanPickup()
    {
        var questSystem = Object.FindAnyObjectByType<QuestSystem>();

        // Se não há quest vinculada, sempre pode coletar
        if (linkedQuest == null || questSystem == null)
        {
            Debug.Log($"[Item3D] Item '{InventoryItem.Name}' pode ser coletado (sem quest vinculada)");
            return true;
        }

        // Verifica se a quest pode ser completada (dependências satisfeitas)
        bool canComplete = questSystem.CanCompleteQuest(linkedQuest);

        Debug.Log($"[Item3D] Verificando se pode coletar '{InventoryItem.Name}' para quest '{linkedQuest.questName}': {canComplete}");

        return canComplete;
    }
}