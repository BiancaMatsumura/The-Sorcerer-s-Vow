using UnityEngine;
using UnityEngine.EventSystems; // Para gerenciar a UI
using UnityEngine.SceneManagement;




public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }


    public GameObject exitConfirmationUI; // Arraste o painel de confirmação no Inspector

    public void ShowExitConfirmation()
    {
        pauseMenuUI.SetActive(false); // Esconde o menu de pausa
        exitConfirmationUI.SetActive(true); // Mostra a tela de confirmação
    }

    public void CancelExit()
    {
        exitConfirmationUI.SetActive(false); // Esconde a tela de confirmação
        pauseMenuUI.SetActive(true); // Volta para o menu de pausa
    }

    public void QuitGame()
    {
        Debug.Log("❌ Fechando jogo...");
        Application.Quit();
    }



    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Resetar para evitar que o menu principal fique pausado
        SceneManager.LoadScene("MainMenu"); // Troque "MainMenu" pelo nome real da cena
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // 🔹 Exibir o cursor e desbloqueá-lo para uso
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; // Permite mover o cursor livremente

        Debug.Log("⏸ Jogo pausado e cursor ativado.");
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // 🔹 Esconder o cursor e bloquear ele no centro (para FPS ou jogos sem mouse)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Trava o cursor no centro da tela

        Debug.Log("▶ Jogo retomado e cursor ocultado.");
    }
}
