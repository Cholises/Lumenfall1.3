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

        if (collision.CompareTag("Enemy"))
        {
            puedeGolpear = false; // evita múltiples golpes por ataque

            Vector2 posicionJugador = transform.root.position;

            // ✔ Enemigo normal
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(danioActual, posicionJugador);

            // ✔ Hongo
            Mushroom mushroom = collision.GetComponent<Mushroom>();
            if (mushroom != null)
                mushroom.TakeDamage(danioActual, posicionJugador);

            // ✔ NUEVO: Jefe NightBorne
            NightBorne night = collision.GetComponent<NightBorne>();
            if (night != null)
                night.TakeDamage(danioActual, posicionJugador);
        }
    }
}
