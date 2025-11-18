using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int danioAtaque1 = 1; // Daño del ataque J
    public int danioAtaque2 = 2; // Daño del ataque K (más fuerte)
    private bool puedeGolpear = false;
    private int danioActual = 1; // El daño que se aplicará actualmente

    // El Samurai llamará este método cuando empiece el ataque
    public void ActivarHitbox(int tipoDanio = 1)
    {
        puedeGolpear = true;
        danioActual = tipoDanio;
        Debug.Log($"✅ Hitbox ACTIVADA - Daño: {danioActual}");
    }

    // El Samurai llamará este método cuando termine el ataque
    public void DesactivarHitbox()
    {
        puedeGolpear = false;
        Debug.Log("❌ Hitbox DESACTIVADA");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"★★★ Sword tocó: {collision.gameObject.name}, Tag: {collision.tag}, PuedeGolpear: {puedeGolpear} ★★★");
        
        if (!puedeGolpear)
        {
            Debug.LogWarning("⚠️ La hitbox NO está activa - No se puede hacer daño");
            return;
        }

        // Detectar enemigo
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("✅ ¡Es un enemigo!");
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Pasar la posición del jugador para calcular la dirección del knockback
                Vector2 posicionJugador = transform.root.position; // Root es el Samurai
                enemy.TakeDamage(danioActual, posicionJugador);
                Debug.Log($"💥 ¡Golpeaste al enemigo! Daño: {danioActual}, Posición jugador: {posicionJugador}");
            }
            else
            {
                Debug.LogError("❌ El objeto con tag Enemy NO tiene el script EnemyController");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ NO es enemigo. Tag encontrado: '{collision.tag}' (esperaba 'Enemy')");
        }
    }
}