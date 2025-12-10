using UnityEngine;

public class PuertaConLlave : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string nombreLlaveRequerida = "LlaveArbol";

    [SerializeField] private BoxCollider2D barrera;
    [SerializeField] private SpriteRenderer spriteBarrera;

    [Header("Visual")]
    [SerializeField] private Color colorBloqueado = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private GameObject particulasBloqueadas;
    [SerializeField] private GameObject efectoDesbloqueo;

    [Header("Mensaje")]
    [SerializeField] private GameObject textoRequerimiento;

    private bool estaBloqueada = true;
    private float tiempoUltimoMensaje = 0f;

    void Start()
    {
        // Asegurar referencia al collider
        if (barrera == null)
            barrera = GetComponent<BoxCollider2D>();

        // Si ya tiene la llave → iniciar desbloqueada
        if (GameManager.Instance != null && GameManager.Instance.TieneLlave(nombreLlaveRequerida))
        {
            DesbloquearPuerta(true); // true = al iniciar
        }
        else
        {
            InicializarPuertaBloqueada();
        }
    }

    void InicializarPuertaBloqueada()
    {
        estaBloqueada = true;

        if (barrera != null)
            barrera.isTrigger = false;

        if (spriteBarrera != null)
            spriteBarrera.color = colorBloqueado;

        if (particulasBloqueadas != null)
            particulasBloqueadas.SetActive(true);
    }

    void Update()
    {
        if (estaBloqueada &&
            GameManager.Instance != null &&
            GameManager.Instance.TieneLlave(nombreLlaveRequerida))
        {
            DesbloquearPuerta(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && estaBloqueada)
        {
            if (Time.time - tiempoUltimoMensaje > 2f)
            {
                MostrarMensaje();
                tiempoUltimoMensaje = Time.time;
            }
        }
    }

    void DesbloquearPuerta(bool iniciandoEscena)
    {
        estaBloqueada = false;

        if (barrera != null)
            barrera.enabled = false;

        if (!iniciandoEscena && efectoDesbloqueo != null)
            Instantiate(efectoDesbloqueo, transform.position, Quaternion.identity);

        if (particulasBloqueadas != null)
            particulasBloqueadas.SetActive(false);

        if (spriteBarrera != null)
            StartCoroutine(FadeOut());
        else
            gameObject.SetActive(false);

        Debug.Log("¡Puerta desbloqueada con llave: " + nombreLlaveRequerida + "!");
    }

    System.Collections.IEnumerator FadeOut()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Color colorInicial = spriteBarrera.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(colorInicial.a, 0f, elapsed / duration);
            spriteBarrera.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    void MostrarMensaje()
    {
        if (textoRequerimiento != null)
        {
            textoRequerimiento.SetActive(true);
            Invoke("OcultarMensaje", 2f);
        }

        Debug.Log("Necesitas la llave: " + nombreLlaveRequerida);
    }

    void OcultarMensaje()
    {
        if (textoRequerimiento != null)
            textoRequerimiento.SetActive(false);
    }
}
