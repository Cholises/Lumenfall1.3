using UnityEngine;

public class MEnemyDZ : MonoBehaviour
{
    private Mushroom mushroom;

    void Start()
    {
        mushroom = GetComponentInParent<Mushroom>();
        
        if (mushroom == null)
            Debug.LogError("EnemyDamageZone no encontró el script Mushroom en el padre!");

        // Evitar errores por escalas del padre
        Vector3 ls = transform.localScale;
        transform.localScale = new Vector3(Mathf.Sign(ls.x), Mathf.Sign(ls.y), 1);

        Debug.Log("DamageZone iniciado correctamente.");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && mushroom != null)
        {
            mushroom.AtacarJugador(collision);
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