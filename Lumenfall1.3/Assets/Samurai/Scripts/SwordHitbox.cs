using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private bool puedeGolpear = false;
    private int danioActual = 1;

    public void ActivarHitbox(int danio)
    {
        puedeGolpear = true;
        danioActual = danio;
    }

    public void DesactivarHitbox()
    {
        puedeGolpear = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!puedeGolpear) return;

        // Acepta enemigos o cualquier objeto que tenga un Hongo
        if (collision.CompareTag("Enemy") || collision.GetComponent<Hongo>() != null)
        {
            puedeGolpear = false;

            Vector2 posicionJugador = transform.root.position;

            // Enemigo controlador genérico
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(danioActual, posicionJugador);

            // Mushroom clásico
            Mushroom mushroom = collision.GetComponent<Mushroom>();
            if (mushroom != null)
                mushroom.TakeDamage(danioActual, posicionJugador);

            // NightBorne jefe
            NightBorne night = collision.GetComponent<NightBorne>();
            if (night != null)
                night.TakeDamage(danioActual, posicionJugador);

            // 🔥 HONGO NUEVO (tu enemigo)
            Hongo h = collision.GetComponent<Hongo>();
            if (h != null)
            {
                h.TakeDamage(danioActual, posicionJugador);
            }
        }
    }
}
