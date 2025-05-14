using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    public string sceneName = "MainMenu";

    public void LoadMenu()
    {
        SceneManager.LoadScene(sceneName);
    }
}
