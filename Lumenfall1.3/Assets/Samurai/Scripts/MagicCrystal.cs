using UnityEngine;

public class KeyUnlock : MonoBehaviour
{
    public string nombreLlave = "LlaveArbol"; 
    public AudioClip pickupSound;

    void Start()
    {
        // Si ya fue obtenida antes → destruir automáticamente
        if (GameManager.Instance != null && GameManager.Instance.TieneLlave(nombreLlave))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Guardar llave permanentemente
            GameManager.Instance.AgregarLlave(nombreLlave);

            // Sonido opcional
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            // Destruir objeto
            Destroy(gameObject);
        }
    }
}
