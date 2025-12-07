using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject pauseButton;

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;  // ← Estaba mal, lo tenías en 0
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        // Recargar la escena actual
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        Time.timeScale = 1;
    }

public void MenuGame()
{
    Debug.Log("Regresaste al menú");

    Time.timeScale = 1;  // ← SUPER IMPORTANTE

    SceneManager.LoadScene("MainMenu");
}

}
