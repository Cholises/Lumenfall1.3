using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Botones del Menú")]
    public Button botonIniciar;
    public Button botonControles;
    public Button botonSalir;

    [Header("Selector Visual")]
    public RectTransform flechaSelector;

    [Header("Paneles")]
    public GameObject panelControles;

    [Header("Navegación")]
    public float tiempoEntreMovimientos = 0.2f;

    [Header("Sonidos")]
    public AudioClip sonidoNavegar; // Sonido al cambiar de opción
    public AudioClip sonidoSeleccionar; // Sonido al confirmar
    public AudioSource audioSource; // Componente AudioSource
    [Range(0f, 1f)]
    public float volumenNavegar = 0.5f;
    [Range(0f, 1f)]
    public float volumenSeleccionar = 0.7f;

    private int indiceSeleccionado = 0;
    private Button[] botones;
    private float ultimoMovimiento = 0f;

    private Coroutine moverFlechaCoroutine;

    void Start()
    {
        botones = new Button[] { botonIniciar, botonControles, botonSalir };

        // Configurar listeners sin recursión
        botonIniciar.onClick.AddListener(() => {
            ReproducirSonido(sonidoSeleccionar, volumenSeleccionar);
            StartCoroutine(CambiarEscenaConRetraso(0.5f));
        });
        
        botonControles.onClick.AddListener(() => {
            ReproducirSonido(sonidoSeleccionar, volumenSeleccionar);
            MostrarControles();
        });
        
        botonSalir.onClick.AddListener(() => {
            ReproducirSonido(sonidoSeleccionar, volumenSeleccionar);
            SalirJuego();
        });

        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ActualizarPosicionFlecha();
    }

    void Update()
    {
        if (Time.time - ultimoMovimiento > tiempoEntreMovimientos)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoverSeleccion(-1);
                ultimoMovimiento = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoverSeleccion(1);
                ultimoMovimiento = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            EjecutarBotonSeleccionado();
        }
    }

    void MoverSeleccion(int direccion)
    {
        indiceSeleccionado += direccion;

        if (indiceSeleccionado < 0)
        {
            indiceSeleccionado = botones.Length - 1;
        }
        else if (indiceSeleccionado >= botones.Length)
        {
            indiceSeleccionado = 0;
        }

        ReproducirSonido(sonidoNavegar, volumenNavegar);
        ActualizarPosicionFlecha();
    }

    void ActualizarPosicionFlecha()
    {
        if (flechaSelector == null) return;

        RectTransform botonRect = botones[indiceSeleccionado].GetComponent<RectTransform>();

        // Calcular borde izquierdo del botón (respecto al Canvas)
        float bordeIzquierdoX = botonRect.anchoredPosition.x - (botonRect.rect.width * botonRect.pivot.x);

        // Posicionar la flecha a la izquierda del borde, con un offset para no tapar texto
        float posicionFlechaX = bordeIzquierdoX - 50f; // Ajusta este valor si quieres

        Vector2 nuevaPosicion = new Vector2(posicionFlechaX, botonRect.anchoredPosition.y);

        if (moverFlechaCoroutine != null)
        {
            StopCoroutine(moverFlechaCoroutine);
        }
        moverFlechaCoroutine = StartCoroutine(MoverFlechaSuave(nuevaPosicion));
    }

    IEnumerator MoverFlechaSuave(Vector2 posicionObjetivo)
    {
        float duracion = 0.15f;
        float tiempoTranscurrido = 0f;
        Vector2 posicionInicial = flechaSelector.anchoredPosition;

        while (tiempoTranscurrido < duracion)
        {
            flechaSelector.anchoredPosition = Vector2.Lerp(posicionInicial, posicionObjetivo, tiempoTranscurrido / duracion);
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        flechaSelector.anchoredPosition = posicionObjetivo;
    }

    void EjecutarBotonSeleccionado()
    {
        // Invocar el onClick del botón seleccionado
        Button boton = botones[indiceSeleccionado];
        boton.onClick.Invoke();
    }

    IEnumerator CambiarEscenaConRetraso(float retraso)
    {
        yield return new WaitForSeconds(retraso);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void ReproducirSonido(AudioClip clip, float volumen)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumen);
        }
    }

    public void MostrarControles()
    {
        Debug.Log("Mostrando controles...");
        if (panelControles != null)
            panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        if (panelControles != null)
            panelControles.SetActive(false);
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}