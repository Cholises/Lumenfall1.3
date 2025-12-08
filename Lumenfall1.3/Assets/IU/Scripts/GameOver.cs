using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{

    public GameObject GameOverPanel;

    // 🔥 Mostrar Game Over SIN puntos
    public void MostrarGameOver()
    {
        Time.timeScale = 0;
        GameOverPanel.SetActive(true);
    }


public void ReiniciarEscena()
{
    Time.timeScale = 1;

    if (GameManager.Instance != null)
    {
        GameManager.Instance.vidaActualJugador = GameManager.Instance.vidaMaximaJugador;
    }

    SceneManager.LoadScene("DirtCave0");
}

public void Menu()
{
    Time.timeScale = 1;

    // 🔥 Resetear la vida ANTES de cambiar de escena
    if (GameManager.Instance != null)
    {
        GameManager.Instance.vidaActualJugador = GameManager.Instance.vidaMaximaJugador;
    }

    SceneManager.LoadScene("MainMenu");
}
}
