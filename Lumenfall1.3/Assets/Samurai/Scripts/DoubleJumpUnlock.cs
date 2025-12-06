using UnityEngine;

public class DoubleJumpUnlock : MonoBehaviour
{
    public AudioClip pickupSound;

    void Start()
    {
        // ✅ Si ya fue desbloqueado antes, destruir este objeto al cargar la escena
        if (GameManager.Instance != null && GameManager.Instance.habilidadDobleSalto)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Samurai player = collision.GetComponent<Samurai>();

            if (player != null)
            {
                // Activar habilidad en Samurai
                player.dobleSaltoHabilitado = true;
            }

            // ✅ Guardar como habilidad permanente
            if (GameManager.Instance != null)
            {
                GameManager.Instance.habilidadDobleSalto = true;
                Debug.Log("🟦 Habilidad de DOBLE SALTO desbloqueada permanentemente");
            }

            // 🔊 Sonido
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // ✅ Destruir el objeto definitivamente
            Destroy(gameObject);
        }
    }
}
