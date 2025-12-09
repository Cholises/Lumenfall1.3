using UnityEngine;

public class HongoDamageZone : MonoBehaviour
{
    private Hongo h;
    private bool canDamage = false;

    void Start()
    {
        h = GetComponentInParent<Hongo>();
    }

    public void EnableDamage()
    {
        canDamage = true;
    }

    public void DisableDamage()
    {
        canDamage = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!canDamage || h == null) return;

        if (other.CompareTag("Player"))
        {
            h.RealizarDaño(other);
        }
    }
}
