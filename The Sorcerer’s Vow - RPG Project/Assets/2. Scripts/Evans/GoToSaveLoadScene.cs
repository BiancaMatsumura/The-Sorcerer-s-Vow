using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSaveScene : MonoBehaviour
{
    public string sceneName = "SaveLoadScene";
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
