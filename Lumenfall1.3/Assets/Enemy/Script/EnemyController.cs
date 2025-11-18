using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;
    public float attackCooldown = 1.5f;
    public float stopDistance = 1.0f; // Distancia a la que se detiene

    private Rigidbody2D rb;
    private Vector2 movement;
    private float lastAttackTime;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Buscar al jugador automáticamente si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Jugador encontrado automáticamente");
            }
            else
            {
                Debug.LogError("No se encuentra el jugador. Asegúrate de que tenga el tag 'Player'");
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Solo perseguir si está en rango Y más lejos que stopDistance
        if (distanceToPlayer < detectionRadius && distanceToPlayer > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);

            // Voltear el sprite según la dirección
            if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1);

            // Actualizar animación si existe
            if (animator != null)
            {
                animator.SetFloat("Speed", Mathf.Abs(direction.x));
            }
        }
        else
        {
            movement = Vector2.zero;
            
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
            }
        }
    }

    void FixedUpdate()
    {
        // Mover en FixedUpdate para mejor física
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    // CORREGIDO: Era "OCollisionEnter2D" (con O mayúscula) 
    // debe ser "OnCollisionEnter2D" (con On)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Solo atacar si ha pasado el cooldown
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Vector2 direccionDanio = transform.position;
                
                Samurai samurai = collision.gameObject.GetComponent<Samurai>();
                if (samurai != null)
                {
                    samurai.RecibeDanio(direccionDanio, 1);
                    lastAttackTime = Time.time;
                    Debug.Log("¡Enemigo atacó al jugador!");
                }
            }
        }
    }

    // Detectar cuando el enemigo deja de tocar al jugador
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Samurai samurai = collision.gameObject.GetComponent<Samurai>();
            if (samurai != null)
            {
                samurai.DesactivaDanio();
            }
            Debug.Log("Enemigo dejó de tocar al jugador - Daño desactivado");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}