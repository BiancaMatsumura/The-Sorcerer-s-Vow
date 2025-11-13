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
        [SerializeField] private int questID; // ← ID da quest que libera a coleta
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
            // Faz o texto/ícone de interação sempre olhar para a câmera
            if (interactionUI != null && mainCamera != null)
            {
                interactionUI.transform.LookAt(mainCamera);
                interactionUI.transform.Rotate(0f, 180f, 0f);
            }
        }

        public void DestroyItem()
        {
            if (isCollected)
                return;

            if (inventoryController == null)
                return;

            var inventory = inventoryController.InventoryData;
            if (inventory == null)
            {
                Debug.LogError("[Item3D] ❌ InventoryData não encontrado!");
                return;
            }

            if (inventory.CanAddItem(InventoryItem, Quantity))
            {
                isCollected = true;
                GetComponent<Collider>().enabled = false;

                Debug.Log($"[Item3D] ✅ Coletando item: {InventoryItem.Name}");

                if (quest != null)
                {
                    // Tenta encontrar o QuestManager ativo na cena
                    var questManager = Object.FindAnyObjectByType<QuestManager>();

                    if (questManager != null)
                    {
                        // Busca a quest ativa correspondente ao ScriptableObject vinculado
                        var activeQuest = questManager.GetQuestByID(quest.id);

                        if (activeQuest != null)
                        {
                            // Marca a missão como completa
                            activeQuest.QuestStatus = QuestStatus.Completed;

                            // Dispara o evento de missão concluída
                            QuestEvents.TriggerQuestCompleted(activeQuest);

                            Debug.Log($"[Item3D] 🧩 Missão '{activeQuest.QuestName}' concluída via coleta do item '{InventoryItem.Name}'.");
                        }
                        else
                        {
                            Debug.LogWarning($"[Item3D] Nenhuma missão ativa encontrada com o ID {quest.id}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[Item3D] Nenhum QuestManager encontrado na cena.");
                    }
                }



                if (particleDisable != null)
                    particleDisable.Clear();

                inventory.AddItem(InventoryItem, Quantity);
                Debug.Log($"[Item3D] Item '{InventoryItem.Name}' adicionado ao inventário");

                StartCoroutine(AnimateItemPickup());
            }
            else
            {
                Debug.Log($"[Item3D] ❌ Inventário cheio — não foi possível coletar '{InventoryItem.Name}'.");
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

            Debug.Log($"[Item3D] 🧹 Destruindo GameObject do item '{InventoryItem.Name}'");
            Destroy(gameObject);
        }

        // ✅ Somente pode pegar se a missão vinculada estiver ativa
        public bool CanPickup()
        {
            // Se não há missão vinculada, pode pegar normalmente
            if (questID <= 0)
                return true;

            var qm = Object.FindAnyObjectByType<QuestManager>();
            if (qm == null)
            {
                Debug.LogWarning("[Item3D] QuestManager não encontrado — liberando coleta.");
                return true;
            }

            var quest = qm.GetQuestByID(questID);
            if (quest == null)
            {
                Debug.LogWarning($"[Item3D] Nenhuma quest encontrada com ID {questID}");
                return false;
            }

            bool canPickup = quest.QuestStatus == QuestStatus.Active;

            Debug.Log($"[Item3D] Checando se pode pegar '{InventoryItem.Name}' — Quest '{quest.QuestName}' está {quest.QuestStatus}. Pode pegar: {canPickup}");

            return canPickup;
        }
    }
}
