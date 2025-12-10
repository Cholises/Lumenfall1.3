using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Botones del Menú")]
    public Button botonIniciar;
    public Button botonSalir;

    [Header("Selector Visual")]
    public RectTransform flechaSelector;

    [Header("Navegación")]
    public float tiempoEntreMovimientos = 0.2f;

    [Header("Sonidos")]
    public AudioClip sonidoNavegar;
    public AudioClip sonidoSeleccionar;
    public AudioSource audioSource;
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
        // Verificar que los botones estén asignados
        if (botonIniciar == null || botonSalir == null)
        {
            Debug.LogError("¡Faltan botones por asignar en el Inspector!");
            return;
        }

        // Ahora solo hay 2 botones
        botones = new Button[] { botonIniciar, botonSalir };

        // Configurar eventos del mouse
        ConfigurarBotonConMouse(botonIniciar, 0, IniciarJuego);
        ConfigurarBotonConMouse(botonSalir, 1, SalirJuego);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        ActualizarPosicionFlecha();
        Debug.Log("MenuManager: Inicializado correctamente");
    }

    void ConfigurarBotonConMouse(Button boton, int indice, UnityEngine.Events.UnityAction accion)
    {
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(() =>
        {
            ReproducirSonido(sonidoSeleccionar, volumenSeleccionar);
            accion();
        });

        EventTrigger trigger = boton.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = boton.gameObject.AddComponent<EventTrigger>();
        else
            trigger.triggers.Clear();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
        pointerEnter.eventID = EventTriggerType.PointerEnter;
        pointerEnter.callback.AddListener((eventData) =>
        {
            if (indiceSeleccionado != indice)
            {
                indiceSeleccionado = indice;
                ReproducirSonido(sonidoNavegar, volumenNavegar);
                ActualizarPosicionFlecha();
            }
        });
        trigger.triggers.Add(pointerEnter);
    }

    void Update()
    {
        // Navegación con teclado
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
            EjecutarBotonSeleccionado();
    }

    void MoverSeleccion(int direccion)
    {
        indiceSeleccionado += direccion;

        if (indiceSeleccionado < 0)
            indiceSeleccionado = botones.Length - 1;
        else if (indiceSeleccionado >= botones.Length)
            indiceSeleccionado = 0;

        ReproducirSonido(sonidoNavegar, volumenNavegar);
        ActualizarPosicionFlecha();
    }

    void ActualizarPosicionFlecha()
    {
        if (flechaSelector == null) return;

        RectTransform botonRect = botones[indiceSeleccionado].GetComponent<RectTransform>();

        float bordeIzquierdoX = botonRect.anchoredPosition.x - (botonRect.rect.width * botonRect.pivot.x);
        float posicionFlechaX = bordeIzquierdoX - 50f;

        Vector2 nuevaPosicion = new Vector2(posicionFlechaX, botonRect.anchoredPosition.y);

        if (moverFlechaCoroutine != null)
            StopCoroutine(moverFlechaCoroutine);

        moverFlechaCoroutine = StartCoroutine(MoverFlechaSuave(nuevaPosicion));
    }

    IEnumerator MoverFlechaSuave(Vector2 posicionObjetivo)
    {
        float duracion = 0.15f;
        float tiempo = 0f;
        Vector2 inicial = flechaSelector.anchoredPosition;

        while (tiempo < duracion)
        {
            flechaSelector.anchoredPosition = Vector2.Lerp(inicial, posicionObjetivo, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        flechaSelector.anchoredPosition = posicionObjetivo;
    }

    void EjecutarBotonSeleccionado()
    {
        botones[indiceSeleccionado].onClick.Invoke();
    }

    void IniciarJuego()
    {
        StartCoroutine(CambiarEscenaConRetraso(0.5f));
    }

    IEnumerator CambiarEscenaConRetraso(float retraso)
    {
        yield return new WaitForSeconds(retraso);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void ReproducirSonido(AudioClip clip, float volumen)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, volumen);
    }

    public void SalirJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
