using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int danioAtaque1 = 1;
    public int danioAtaque2 = 2;
    private bool puedeGolpear = false;
    private int danioActual = 1;

    public void ActivarHitbox(int tipoDanio = 1)
    {
        puedeGolpear = true;
        danioActual = tipoDanio;
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
            Vector2 posicionJugador = transform.root.position;
            
            // Detectar enemigo volador
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(danioActual, posicionJugador);
            }
            
            // Detectar Mushroom
            Mushroom Mushroom = collision.GetComponent<Mushroom>();
            if (Mushroom != null)
            {
                Mushroom.TakeDamage(danioActual, posicionJugador);
            }
        }
    }
}