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
        
        if (hongo == null)
        {
            Debug.LogError("HongoDamageZone: No se encontró el componente Hongo en el padre!");
        }
        
        Debug.Log("HongoDamageZone iniciado en: " + gameObject.name);
    }

    public void EnableDamage()
    {
        canDamage = true;
        damagedTargets.Clear();
        Debug.Log("HongoDamageZone: Daño ACTIVADO - Esperando colisiones...");
    }

    public void DisableDamage()
    {
        canDamage = false;
        damagedTargets.Clear();
        Debug.Log("HongoDamageZone: Daño DESACTIVADO");
    }

    // USAMOS TODOS LOS MÉTODOS DE TRIGGER PARA DIAGNÓSTICO
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(">>> TRIGGER ENTER con: " + other.name + " | Tag: " + other.tag + " | canDamage: " + canDamage);
        ProcessCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (canDamage && other.CompareTag("Player"))
        {
            Debug.Log(">>> TRIGGER STAY con Player: " + other.name);
        }
        ProcessCollision(other);
    }

    private void ProcessCollision(Collider2D other)
    {
        if (!canDamage)
        {
            return;
        }

        if (hongo == null)
        {
            Debug.LogError("HongoDamageZone: hongo es NULL!");
            return;
        }

        // Verificar si ya dañamos a este objetivo
        if (damagedTargets.Contains(other))
        {
            return;
        }

        Debug.Log("Procesando colisión con: " + other.name + " (Tag: " + other.tag + ")");

        // VERIFICAR SI ES EL PLAYER
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡¡¡ ES EL PLAYER !!!");
            
            // Verificar que tenga el componente Samurai
            Samurai sam = other.GetComponent<Samurai>();
            if (sam != null)
            {
                Debug.Log("★★★ DAÑO APLICADO AL SAMURAI ★★★");
                damagedTargets.Add(other);
                hongo.RealizarDaño(other);
            }
            else
            {
                Debug.LogError("Tiene tag Player pero NO tiene componente Samurai!");
            }
        }
        else
        {
            Debug.Log("NO es Player, es: " + other.tag);
        }
    }

    // Ver el área de daño en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = canDamage ? Color.red : Color.yellow;
        
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
        }
        
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
    }
}