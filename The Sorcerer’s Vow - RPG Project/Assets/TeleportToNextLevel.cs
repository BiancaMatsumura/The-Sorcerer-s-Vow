using UnityEngine;

public class TeleportToNextLevel : MonoBehaviour
{
    private FadeTransition fade;

    private void Start()
    {
        fade = FindFirstObjectByType<FadeTransition>();
        if (fade == null)
            Debug.LogError("FadeTransition não encontrado na cena!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            fade.nextSceneName = "Zigurat_Fase1"; // ou o nome correto da próxima cena
            fade.StartFade();
        }
    }
}
