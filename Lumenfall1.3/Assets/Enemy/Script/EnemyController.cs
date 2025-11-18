using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Movimiento")]
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;
    public float stopDistance = 1.5f; // Aumentado para que ataque desde más lejos
    private Vector2 movement;
    private bool isKnockedBack = false; // NUEVO: Para saber si está en knockback

    [Header("Combate")]
    public float attackCooldown = 1.5f;
    public int health = 3;
    public float knockbackForce = 8f; // Fuerza del empuje al recibir daño
    private float lastAttackTime;
    private bool isDead = false;

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
        if (player == null || isDead || isKnockedBack) return; // No actuar durante knockback

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si está en rango de ataque (cerca)
        if (distanceToPlayer <= stopDistance)
        {
            movement = Vector2.zero;
            
            // Activar animación de ataque
            if (animator != null)
            {
                animator.SetBool("Attack", true);
            }
        }
        // Si está en rango de detección pero lejos (perseguir)
        else if (distanceToPlayer < detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);

            // Voltear el sprite según la dirección
            if (direction.x < 0)
                transform.localScale = new Vector3(-2, 2, 1);
            else if (direction.x > 0)
                transform.localScale = new Vector3(2, 2, 1);

            // Desactivar ataque, mantener vuelo
            if (animator != null)
            {
                animator.SetBool("Attack", false);
            }
        }
        else
        {
            // Fuera de rango - idle/vuelo
            movement = Vector2.zero;
            
            if (animator != null)
            {
                animator.SetBool("Attack", false);
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead || isKnockedBack) return; // No mover durante knockback
        
        // Mover en FixedUpdate para mejor física
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
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

    // Método público llamado por EnemyDamageZone
    public void AtacarJugador(Collider2D jugador)
    {
        if (isDead) return;

        // Atacar continuamente con cooldown
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Vector2 direccionDanio = transform.position;
            
            Samurai samurai = jugador.GetComponent<Samurai>();
            if (samurai != null)
            {
                // NUEVO: Activar animación de ataque antes de hacer daño
                if (animator != null)
                {
                    animator.SetTrigger("Attack"); // Usar trigger en lugar de bool
                }
                
                samurai.RecibeDanio(direccionDanio, 1);
                lastAttackTime = Time.time;
                Debug.Log("¡Enemigo atacó al jugador!");
            }
        }
    }

    // Sobrecarga: Método para recibir daño SIN knockback (para compatibilidad)
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position); // Llamar a la versión completa sin dirección específica
    }

    // Método para recibir daño CON knockback (llamar desde el script del jugador cuando ataque)
    public void TakeDamage(int damage, Vector2 direccionGolpe)
    {
        if (isDead) return;

        health -= damage;
        
        // Activar animación de Hit
        if (animator != null)
        {
            animator.SetBool("Hit", true);
            StartCoroutine(ResetHit());
        }

        // Aplicar knockback (empuje hacia atrás)
        if (rb != null)
        {
            // Detener el movimiento actual
            movement = Vector2.zero;
            
            // Calcular dirección opuesta al golpe (solo horizontal para voladores)
            float direccionX = transform.position.x - direccionGolpe.x;
            Vector2 direccionKnockback = new Vector2(Mathf.Sign(direccionX), 0.3f).normalized;
            
            // Aplicar fuerza de empuje más fuerte
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(direccionKnockback * knockbackForce, ForceMode2D.Impulse);
            
            // Iniciar corrutina para recuperar el control después del knockback
            StartCoroutine(KnockbackRecovery());
            
            Debug.Log($"Enemigo recibió knockback. Dirección: {direccionKnockback}, Fuerza: {knockbackForce}");
        }

        Debug.Log($"Enemigo recibió {damage} de daño. Vida restante: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator KnockbackRecovery()
    {
        // Activar estado de knockback
        isKnockedBack = true;
        
        // Desactivar movimiento durante el knockback
        float tempSpeed = speed;
        speed = 0;
        
        yield return new WaitForSeconds(0.3f);
        
        // Restaurar movimiento y estado
        speed = tempSpeed;
        rb.linearVelocity = Vector2.zero; // Detener completamente después del knockback
        isKnockedBack = false;
    }

    IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.3f); // Ajusta según duración de tu animación Hit
        if (animator != null)
        {
            animator.SetBool("Hit", false);
        }
    }

    void Die()
    {
        isDead = true;
        
        // Activar animación de muerte
        if (animator != null)
        {
            animator.SetBool("Death", true);
            animator.SetBool("Attack", false);
            animator.SetBool("Hit", false);
        }

        // Desactivar componentes
       rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Debug.Log("Enemigo ha muerto");

        // Destruir después de la animación
        Destroy(gameObject, 2f); // Ajusta el tiempo según tu animación
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}