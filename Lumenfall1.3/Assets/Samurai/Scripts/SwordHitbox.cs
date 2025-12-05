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
            puedeGolpear = false; // evita múltiples daños por ataque

            Vector2 posicionJugador = transform.root.position;

            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(danioActual, posicionJugador);

            Mushroom mushroom = collision.GetComponent<Mushroom>();
            if (mushroom != null)
                mushroom.TakeDamage(danioActual, posicionJugador);
        }
    }
}
