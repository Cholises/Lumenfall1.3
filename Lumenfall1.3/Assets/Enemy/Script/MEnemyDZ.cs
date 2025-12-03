using UnityEngine;

public class MEnemyDZ : MonoBehaviour
{
    private Mushroom mush;
    private bool playerInside = false;

    void Start()
    {
        mush = GetComponentInParent<Mushroom>();

        if (mush == null)
        {
            Debug.LogError("MEnemyDZ: No se encontró el componente MushroomEnemy en el padre!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!playerInside || mush == null) return;

        if (other.CompareTag("Player"))
        {
            mush.ForzarAtaque(other);  // Nuevo método especial para zonas
        }
    }
}
