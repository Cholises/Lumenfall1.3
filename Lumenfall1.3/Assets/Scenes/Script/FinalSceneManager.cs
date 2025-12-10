using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalSceneManager : MonoBehaviour
{
    [Header("Tiempo antes de volver al menú")]
    public float tiempoEspera = 5f; // segundos
    public string escenaMenu = "MainMenu"; // nombre exacto de tu escena de menú

    void Start()
    {
        StartCoroutine(VolverAlMenu());
    }

    System.Collections.IEnumerator VolverAlMenu()
    {
        yield return new WaitForSeconds(tiempoEspera);
        SceneManager.LoadScene(escenaMenu);
    }
}
