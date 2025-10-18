using UnityEngine;

public class TeleportToNextLevel : MonoBehaviour
{
    private FadeTransition fade;

    [SerializeField]
    private QuestData lastQuest;

    private void Start()
    {
        fade = FindFirstObjectByType<FadeTransition>();
        if (fade == null)
            Debug.LogError("FadeTransition não encontrado na cena!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")&& lastQuest != null && lastQuest.isCompleted)
        {
            fade.nextSceneName = "BossTest"; // ou o nome correto da próxima cena
            fade.StartFade();
        }
    }
}
