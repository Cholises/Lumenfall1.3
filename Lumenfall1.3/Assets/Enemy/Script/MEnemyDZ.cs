using UnityEngine;

public class MEnemyDZ : MonoBehaviour
{
    private Mushroom mush;

    void Start()
    {
        mush = GetComponentInParent<Mushroom>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mush.EntrarZona();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mush.SalirZona();
        }
    }
}
