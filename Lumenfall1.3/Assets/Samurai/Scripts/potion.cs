using UnityEngine;

public class Potion : MonoBehaviour
{
    [Header("Curación")]
    public int cantidadCuracion = 2;

    [Header("Efectos")]
    [SerializeField] private AudioClip sonidoRecoger; // ← agrega el sonido aquí
    [SerializeField] private float volumen = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Samurai samurai = collision.GetComponent<Samurai>();

        if (samurai != null)
        {
            // Curar al jugador
            samurai.Curar(cantidadCuracion);

            // Reproducir sonido en la posición de la poción
            if (sonidoRecoger != null)
                AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position, volumen);

            // Destruir la poción
            Destroy(gameObject);
        }
    }
}
