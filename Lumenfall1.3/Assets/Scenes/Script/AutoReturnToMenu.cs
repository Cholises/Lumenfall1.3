using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoReturnToMenu : MonoBehaviour
{
    public float tiempoEspera = 5f;

    void Start()
    {
        Invoke("RegresarAlMenu", tiempoEspera);
    }

    void RegresarAlMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
