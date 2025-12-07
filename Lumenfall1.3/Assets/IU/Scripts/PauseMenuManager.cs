using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseButton;
    private bool isPaused = false;

    void Update()
    {
        // Detectar cuando se presiona la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        pauseMenu.SetActive(true);
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;  
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);
        isPaused = false;
    }

    public void RestartGame()
    {
        // Recargar la escena actual
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        Time.timeScale = 1;
        isPaused = false;
    }

    public void MenuGame()
    {
        Debug.Log("Regresaste al menú");

        Time.timeScale = 1;  // ← SUPER IMPORTANTE

        SceneManager.LoadScene("MainMenu");
        isPaused = false;
    }
}