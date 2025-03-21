using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void SairJogo()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}