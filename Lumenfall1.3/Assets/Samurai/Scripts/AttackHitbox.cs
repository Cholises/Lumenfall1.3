using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [Header("Configuración")]
    public int damage = 2;
    public float knockbackForce = 3f;
    
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    void OnEnable()
    {
        hitEnemies.Clear();
        Debug.Log($"✅ [{gameObject.name}] Hitbox activada - Daño: {damage}");
    }

    void OnDisable()
    {
        Debug.Log($"❌ [{gameObject.name}] Hitbox desactivada");
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"🎯 [{gameObject.name}] Detectó: {collision.name} (Tag: {collision.tag})");
        
        // Evitar golpear al mismo enemigo varias veces en este ataque
        if (hitEnemies.Contains(collision))
        {
            Debug.Log($"⚠️ Ya golpeado: {collision.name}");
            return;
        }

        // Acepta enemigos o cualquier objeto que tenga un Hongo
        if (collision.CompareTag("Enemy") || collision.GetComponent<Hongo>() != null)
        {
            hitEnemies.Add(collision);
            
            Vector2 posicionJugador = transform.root.position;

            Debug.Log($"💥 [{gameObject.name}] ¡GOLPE a {collision.name}! Daño: {damage}");

            // Enemigo controlador genérico
            EnemyController enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, posicionJugador);
                Debug.Log($"✅ Daño aplicado a EnemyController");
                return;
            }

            // Mushroom clásico
            Mushroom mushroom = collision.GetComponent<Mushroom>();
            if (mushroom != null)
            {
                mushroom.TakeDamage(damage, posicionJugador);
                Debug.Log($"✅ Daño aplicado a Mushroom");
                return;
            }

            // NightBorne jefe
            NightBorne night = collision.GetComponent<NightBorne>();
            if (night != null)
            {
                night.TakeDamage(damage, posicionJugador);
                Debug.Log($"✅ Daño aplicado a NightBorne");
                return;
            }

            // 🔥 HONGO NUEVO (tu enemigo)
            Hongo h = collision.GetComponent<Hongo>();
            if (h != null)
            {
                h.TakeDamage(damage, posicionJugador);
                Debug.Log($"✅ Daño aplicado a Hongo");
                return;
            }

            Debug.LogWarning($"⚠️ {collision.name} tiene tag Enemy pero no tiene script de daño");
        }
        else
        {
            Debug.Log($"⚠️ {collision.name} no es Enemy ni Hongo");
        }
    }
}
