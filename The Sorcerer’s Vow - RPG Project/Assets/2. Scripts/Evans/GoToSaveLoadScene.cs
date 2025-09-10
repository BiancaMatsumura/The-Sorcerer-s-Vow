using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSaveScene : MonoBehaviour
{
    public string sceneName = "SaveLoadScene";
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Nao tem cena definida ainda via Inspector!");
        }
    }
}
