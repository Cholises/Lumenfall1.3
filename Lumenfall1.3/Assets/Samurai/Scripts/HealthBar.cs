using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Referencias")]
    public Samurai samurai;
    public Image barraVida;
    public Image barraFondo;

    [Header("Colores")]
    public Color colorVidaAlta = Color.red;
    public Color colorVidaMedia = Color.yellow;
    public Color colorVidaBaja = Color.red;

    [Header("Animación")]
    public bool usarAnimacionSuave = true;
    public float velocidadAnimacion = 5f;

    private float vidaObjetivo;

    void Start()
    {
        if (samurai == null)
        {
            samurai = FindFirstObjectByType<Samurai>();
            if (samurai == null)
            {
                Debug.LogError("No se encontró el Samurai en la escena");
                return;
            }
        }

        // CORREGIDO: Inicializar con la vida ACTUAL del Samurai
        if (barraVida != null && samurai != null)
        {
            float vidaActual = samurai.ObtenerVidaActual();
            float vidaMaxima = samurai.ObtenerVidaMaxima();
            float porcentajeVida = vidaActual / vidaMaxima;
            
            barraVida.fillAmount = porcentajeVida; // Cambio importante aquí
            barraVida.color = colorVidaAlta;
            vidaObjetivo = porcentajeVida; // Y aquí
            
            Debug.Log($"HealthBar inicializada: {vidaActual}/{vidaMaxima} = {porcentajeVida}");
        }
    }

    void Update()
    {
        if (samurai == null || barraVida == null) return;

        float vidaActual = samurai.ObtenerVidaActual();
        float vidaMaxima = samurai.ObtenerVidaMaxima();
        vidaObjetivo = vidaActual / vidaMaxima;

        if (usarAnimacionSuave)
        {
            barraVida.fillAmount = Mathf.Lerp(barraVida.fillAmount, vidaObjetivo, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            barraVida.fillAmount = vidaObjetivo;
        }

        ActualizarColor();
    }

    void ActualizarColor()
    {
        if (barraVida == null) return;
        barraVida.color = Color.red;
    }

    public void AnimarDanio()
    {
        StartCoroutine(ParpadeoDanio());
    }

    System.Collections.IEnumerator ParpadeoDanio()
    {
        if (barraVida == null) yield break;

        Color colorOriginal = barraVida.color;
        
        for (int i = 0; i < 3; i++)
        {
            barraVida.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            barraVida.color = colorOriginal;
            yield return new WaitForSeconds(0.1f);
        }
    }
}