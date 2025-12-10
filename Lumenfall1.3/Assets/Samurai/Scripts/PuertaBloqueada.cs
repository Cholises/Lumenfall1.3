using UnityEngine;

public class PuertaBloqueada : MonoBehaviour
{
    [Header("Configuración")]
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
        // Inicializar la barrera como bloqueada
        if (barrera == null)
            barrera = GetComponent<BoxCollider2D>();
        
        if (barrera != null)
        {
            barrera.isTrigger = false; // Collider sólido que bloquea
        }
        
        // Color visual de bloqueado
        if (spriteBarrera != null)
        {
            spriteBarrera.color = colorBloqueado;
        }
        
        if (particulasBloqueadas != null)
        {
            particulasBloqueadas.SetActive(true);
        }
    }

    void Update()
    {
        // Verificar constantemente si el jugador ya tiene la habilidad
        if (estaBloqueada && GameManager.Instance.TieneDoubleJump())
        {
            DesbloquearPuerta();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el jugador choca y la puerta está bloqueada, mostrar mensaje
        if (collision.gameObject.CompareTag("Player") && estaBloqueada)
        {
            // Evitar spam de mensajes
            if (Time.time - tiempoUltimoMensaje > 2f)
            {
                MostrarMensaje();
                tiempoUltimoMensaje = Time.time;
            }
        }
    }

    void DesbloquearPuerta()
    {
        estaBloqueada = false;
        
        // Desactivar el collider que bloquea
        if (barrera != null)
        {
            barrera.enabled = false;
        }
        
        // Efecto visual de desbloqueo
        if (efectoDesbloqueo != null)
        {
            Instantiate(efectoDesbloqueo, transform.position, Quaternion.identity);
        }
        
        // Ocultar partículas de bloqueado
        if (particulasBloqueadas != null)
        {
            particulasBloqueadas.SetActive(false);
        }
        
        // Hacer desaparecer la barrera visual
        if (spriteBarrera != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            gameObject.SetActive(false);
        }
        
        Debug.Log("¡Puerta desbloqueada!");
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
        
        Debug.Log("Necesitas el Double Jump para pasar");
    }

    void OcultarMensaje()
    {
        if (textoRequerimiento != null)
        {
            textoRequerimiento.SetActive(false);
        }
    }
}