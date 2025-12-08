using UnityEngine;

public class NightBorneDZ : MonoBehaviour
{
    private NightBorne nb;

    void Start()
    {
        nb = GetComponentInParent<NightBorne>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            nb.ActivarCombate();   // 🔥 Entra en modo combate
    }
}
