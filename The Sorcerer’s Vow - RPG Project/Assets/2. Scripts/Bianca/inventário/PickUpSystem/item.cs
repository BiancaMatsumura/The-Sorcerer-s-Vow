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
        [SerializeField] private int dependencyQuestID; // ⭐ NOVO — Missão que precisa estar COMPLETA
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

            // Se não há dependência, libera
            if (dependencyQuestID <= 0)
                return true;

            var depQuest = qm.GetQuestByID(dependencyQuestID);

            if (depQuest == null)
            {
                Debug.LogWarning($"[Item3D] Missão dependência {dependencyQuestID} não encontrada.");
                return false;
            }

            bool canPickup = depQuest.QuestStatus == QuestStatus.Completed;

            if (!canPickup)
            {
                Debug.Log($"[Item3D] BLOQUEADO: item '{InventoryItem.Name}' depende da missão {depQuest.QuestName} COMPLETA.");
            }

            return canPickup;
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

                Debug.Log($"[Item3D] Coletando item: {InventoryItem.Name}");

                // --- Completa missão vinculada opcionalmente ---
                if (quest != null)
                {
                    var questManager = Object.FindAnyObjectByType<QuestManager>();
                    var activeQuest = questManager?.GetQuestByID(quest.id);

                    if (activeQuest != null)
                    {
                        activeQuest.QuestStatus = QuestStatus.Completed;
                        QuestEvents.TriggerQuestCompleted(activeQuest);
                    }
                }

                if (particleDisable != null)
                    particleDisable.Clear();

                inventory.AddItem(InventoryItem, Quantity);

                StartCoroutine(AnimateItemPickup());
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
