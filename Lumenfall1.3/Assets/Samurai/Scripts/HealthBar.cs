using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Referencias")]
    public Samurai samurai; // Referencia al jugador
    public Image barraVida; // La imagen roja que se reduce
    public Image barraFondo; // Opcional: fondo gris/negro

    [Header("Colores")]
    public Color colorVidaAlta = Color.red;      // CAMBIADO: Rojo cuando está llena
    public Color colorVidaMedia = Color.yellow;
    public Color colorVidaBaja = Color.red;

    [Header("Animación")]
    public bool usarAnimacionSuave = true;
    public float velocidadAnimacion = 5f;

    private float vidaObjetivo;

    void Start()
    {
        // Buscar automáticamente al Samurai si no está asignado
        if (samurai == null)
        {
            samurai = FindFirstObjectByType<Samurai>();
            if (samurai == null)
            {
                Debug.LogError("No se encontró el Samurai en la escena");
                return;
            }
        }

        // Inicializar la barra llena y ROJA
        if (barraVida != null)
        {
            barraVida.fillAmount = 1f;
            barraVida.color = colorVidaAlta; // NUEVO: Asegurar que empiece roja
            vidaObjetivo = 1f;
        }
    }

    void Update()
    {
        if (samurai == null || barraVida == null) return;

        // Calcular el porcentaje de vida
        float vidaActual = samurai.ObtenerVidaActual();
        float vidaMaxima = samurai.ObtenerVidaMaxima();
        vidaObjetivo = vidaActual / vidaMaxima;

        // Animar la barra suavemente o cambiar instantáneamente
        if (usarAnimacionSuave)
        {
            barraVida.fillAmount = Mathf.Lerp(barraVida.fillAmount, vidaObjetivo, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            barraVida.fillAmount = vidaObjetivo;
        }

        // Cambiar color según la vida
        ActualizarColor();
    }

    void ActualizarColor()
    {
        if (barraVida == null) return;

        // SIEMPRE mantener la barra ROJA sin importar la vida
        barraVida.color = Color.red;
        
        /* CÓDIGO ANTERIOR (causaba el problema):
        // Cambiar color según el porcentaje de vida
        if (vidaObjetivo > 0.6f)
        {
            barraVida.color = colorVidaAlta; // Verde si tiene más del 60%
        }
        else if (vidaObjetivo > 0.3f)
        {
            barraVida.color = colorVidaMedia; // Amarillo entre 30%-60%
        }
        else
        {
            barraVida.color = colorVidaBaja; // Rojo si tiene menos del 30%
        }
        */
    }

    // Método opcional para hacer que la barra parpadee cuando recibe daño
    public void AnimarDanio()
    {
        StartCoroutine(ParpadeoDanio());
    }

    System.Collections.IEnumerator ParpadeoDanio()
    {
        if (barraVida == null) yield break;

        Color colorOriginal = barraVida.color;
        
        // Parpadear 3 veces
        for (int i = 0; i < 3; i++)
        {
            barraVida.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            barraVida.color = colorOriginal;
            yield return new WaitForSeconds(0.1f);
        }
    }
}