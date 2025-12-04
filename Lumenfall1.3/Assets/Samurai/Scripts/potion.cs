using UnityEngine;

public class Potion : MonoBehaviour
{
    public int cantidadCuracion = 2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Samurai samurai = collision.GetComponent<Samurai>();

        if (samurai != null)
        {
            samurai.Curar(cantidadCuracion);
            Destroy(gameObject); // Desaparece la poción
        }
    }
}
