using QuestSystem;
using UnityEngine;

public class TeleportToNextLevel : MonoBehaviour
{
    private FadeTransition fade;

    [Header("Vinculação de Missão")]
    [SerializeField] private int questID;  // ID da quest que habilita o teleporte
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private string nextSceneName = "BossTest";

    private QuestManager questManager;
    private Quest linkedQuest;

    private void Start()
    {
        fade = FindFirstObjectByType<FadeTransition>();
        if (fade == null)
            Debug.LogError("FadeTransition não encontrado!");

        // Busca o QuestManager
        questManager = FindFirstObjectByType<QuestManager>();
        if (questManager == null)
        {
            Debug.LogError("QuestManager não encontrado na cena!");
            return;
        }

        // Pega a quest pelo ID
        if (questManager.Quests.TryGetValue(questID, out linkedQuest) == false)
        {
            Debug.LogError($"A Quest ID {questID} não foi encontrada no QuestManager!");
        }

        if (sceneLoader == null)
            sceneLoader = FindFirstObjectByType<SceneLoader>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Impede erros
        if (linkedQuest == null)
            return;

        // Verifica se o player encostou e a quest está concluída
        if (other.CompareTag("Player") &&
            linkedQuest.QuestStatus == QuestStatus.Completed)
        {
            StartCoroutine(FadeAndLoad());
        }
    }

    private System.Collections.IEnumerator FadeAndLoad()
    {
        fade.StartFade();
        yield return new WaitForSeconds(fade.fadeDuration);

        if (sceneLoader != null)
        {
            sceneLoader.sceneName = nextSceneName;
            sceneLoader.LoadScene();
        }
        else
        {
            Debug.LogError("SceneLoader não está referenciado!");
        }
    }
}
