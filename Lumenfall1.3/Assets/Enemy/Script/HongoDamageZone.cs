using UnityEngine;
using System.Collections.Generic;

public class HongoDamageZone : MonoBehaviour
{
    private Hongo hongo;
    private bool canDamage = false;
    private HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

    void Start()
    {
        hongo = GetComponentInParent<Hongo>();
    }

    public void EnableDamage()
    {
        canDamage = true;
        damagedTargets.Clear();
    }

    public void DisableDamage()
    {
        canDamage = false;
        damagedTargets.Clear();
    }
    private void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log("El hongo detectó: " + other.name);

    if (!canDamage || hongo == null) return;

    if (other.CompareTag("Player"))
    {
        Debug.Log("¡Golpe al player!");
        hongo.RealizarDaño(other);
    }
}
}
