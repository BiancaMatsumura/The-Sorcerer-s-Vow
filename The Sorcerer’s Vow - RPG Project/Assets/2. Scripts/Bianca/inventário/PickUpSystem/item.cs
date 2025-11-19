using System.Collections;
using UnityEngine;
using Inventory.Model;
using QuestSystem;

namespace QuestSystem
{
    public class Item3D : MonoBehaviour
    {
        [field: SerializeField] public ItemSO InventoryItem { get; private set; }
        [field: SerializeField] public int Quantity { get; set; } = 1;

        [Header("Vinculação de Missão")]
        [SerializeField] private int questID; // Missão que libera a coleta quando está ATIVA
        [SerializeField] private int dependencyQuestID; // ⭐ Missão que precisa estar COMPLETA
        public SO_Quest quest;

        [Header("Efeitos e UI")]
        public AudioSource audioSource;
        [SerializeField] private ParticleSystem particleDisable;
        [SerializeField] private GameObject interactionUI;
        [SerializeField] private Animation anim;

        [Header("Animação")]
        [SerializeField] private float duration = 0.3f;

        private InventoryController inventoryController;
        private Transform mainCamera;
        private bool isCollected = false;

        private void Awake()
        {
            mainCamera = Camera.main?.transform;
            anim ??= GetComponent<Animation>();
            inventoryController = Object.FindAnyObjectByType<InventoryController>();

            if (inventoryController == null)
                Debug.LogError("[Item3D] ❌ InventoryController não encontrado!");
        }

        private void Update()
        {
            if (interactionUI != null && mainCamera != null)
            {
                interactionUI.transform.LookAt(mainCamera);
                interactionUI.transform.Rotate(0f, 180f, 0f);
            }
        }

        public bool CanPickup()
        {
            var qm = Object.FindAnyObjectByType<QuestManager>();
            if (qm == null)
            {
                Debug.LogWarning("[Item3D] QuestManager não encontrado — permitindo coleta.");
                return true;
            }

            // ⭐ 1) Se há uma missão que deve estar ATIVA
            if (questID > 0)
            {
                var requiredQuest = qm.GetQuestByID(questID);

                if (requiredQuest == null)
                {
                    Debug.LogWarning($"[Item3D] Missão requerida {questID} não encontrada.");
                    return false;
                }

                if (requiredQuest.QuestStatus != QuestStatus.Active)
                {
                    Debug.Log($"[Item3D] ❌ BLOQUEADO: item '{InventoryItem.Name}' só pode ser pego quando a missão '{requiredQuest.QuestName}' estiver ATIVA.");
                    return false;
                }
            }

            // ⭐ 2) Se há missão dependência que precisa estar COMPLETA
            if (dependencyQuestID > 0)
            {
                var depQuest = qm.GetQuestByID(dependencyQuestID);

                if (depQuest == null)
                {
                    Debug.LogWarning($"[Item3D] Missão dependência {dependencyQuestID} não encontrada.");
                    return false;
                }

                if (depQuest.QuestStatus != QuestStatus.Completed)
                {
                    Debug.Log($"[Item3D] ❌ BLOQUEADO: item '{InventoryItem.Name}' depende da missão '{depQuest.QuestName}' COMPLETA.");
                    return false;
                }
            }

            return true;
        }


        public void DestroyItem()
        {
            if (isCollected || inventoryController == null)
                return;

            var inventory = inventoryController.InventoryData;
            if (inventory == null)
                return;

            // ❗ Se não pode pegar, não coleta
            if (!CanPickup())
            {
                Debug.Log($"[Item3D] ❌ Coleta bloqueada para '{InventoryItem.Name}' devido às regras de missão.");
                return;
            }

            if (inventory.CanAddItem(InventoryItem, Quantity))
            {
                isCollected = true;
                GetComponent<Collider>().enabled = false;

                Debug.Log($"[Item3D] ✅ Coletando item: {InventoryItem.Name}");

                // 🔥 CORRIGIDO: Usa o método Complete() ao invés de mudar o status diretamente
                if (quest != null)
                {
                    var questManager = Object.FindAnyObjectByType<QuestManager>();
                    var activeQuest = questManager?.GetQuestByID(quest.id);

                    if (activeQuest != null && activeQuest.QuestStatus == QuestStatus.Active)
                    {
                        Debug.Log($"[Item3D] 🏁 Completando quest '{activeQuest.QuestName}' (ID: {quest.id})");
                        activeQuest.Complete(); // ✅ Chama o método correto
                    }
                    else if (activeQuest != null)
                    {
                        Debug.LogWarning($"[Item3D] ⚠ Quest '{activeQuest.QuestName}' não está ativa (Status: {activeQuest.QuestStatus})");
                    }
                }

                if (particleDisable != null)
                    particleDisable.Clear();

                inventory.AddItem(InventoryItem, Quantity);

                StartCoroutine(AnimateItemPickup());
            }
            else
            {
                Debug.LogWarning($"[Item3D] ⚠ Inventário cheio! Não foi possível adicionar '{InventoryItem.Name}'");
            }
        }

        private IEnumerator AnimateItemPickup()
        {
            if (audioSource != null)
                audioSource.Play();

            if (anim != null)
                anim.Stop();

            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.zero;
            float currentTime = 0;

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
    }
}