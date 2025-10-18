using UnityEngine;

public class GameOverBoss_Queda : MonoBehaviour
{
    public GameObject gameOverUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Invoke("GameOver", 3f);
        }
    }


    public void GameOver()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f; 
    }
    
}
