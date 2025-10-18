using UnityEngine;

public class TeleportToNextLevel : MonoBehaviour
{
    private FadeTransition fade;

    [SerializeField] private QuestData lastQuest;
    [SerializeField] private SceneLoader sceneLoader; // 🔹 referência ao SceneLoader
    [SerializeField] private string nextSceneName = "BossTest"; // nome da próxima cena

    private void Start()
    {
        fade = FindFirstObjectByType<FadeTransition>();
        if (fade == null)
            Debug.LogError("FadeTransition não encontrado na cena!");

        if (sceneLoader == null)
            sceneLoader = FindFirstObjectByType<SceneLoader>(); // tenta achar automaticamente
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && lastQuest != null && lastQuest.isCompleted)
        {
            StartCoroutine(FadeAndLoad());
        }
    }

    private System.Collections.IEnumerator FadeAndLoad()
    {
        fade.StartFade();

        // Espera o fade terminar (ajuste o tempo conforme o seu FadeTransition)
        yield return new WaitForSeconds(fade.fadeDuration);

        if (sceneLoader != null)
        {
            sceneLoader.sceneName = nextSceneName;
            sceneLoader.LoadScene(); // 🔹 chama o carregamento assíncrono
        }
        else
        {
            Debug.LogError("SceneLoader não está referenciado!");
        }
    }
}
