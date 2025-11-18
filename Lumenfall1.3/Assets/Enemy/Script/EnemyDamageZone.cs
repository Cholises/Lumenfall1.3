using UnityEngine;

public class EnemyDamageZone : MonoBehaviour
{
    private EnemyController enemyController;

    void Start()
    {
        // Obtener referencia al script del padre (Enemy)
        enemyController = GetComponentInParent<EnemyController>();
        
        if (enemyController == null)
        {
            Debug.LogError("EnemyDamageZone no encontró EnemyController en el padre!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && enemyController != null)
        {
            enemyController.AtacarJugador(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Samurai samurai = collision.GetComponent<Samurai>();
            if (samurai != null)
            {
                samurai.DesactivaDanio();
                Debug.Log("Jugador salió de la zona de daño del enemigo");
            }
        }
    }
}