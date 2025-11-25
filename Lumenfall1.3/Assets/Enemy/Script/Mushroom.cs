using UnityEngine;
using System.Collections;

public class Mushroom : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Movimiento")]
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;
    public float stopDistance = 1.5f;
    public float patrolSpeed = 1.5f;
    public float patrolDistance = 3f;
    private Vector2 movement;
    private bool isKnockedBack = false;
    
    // Variables de patrullaje
    private Vector3 startPosition;
    private float patrolDirection = 1f;
    private bool isPatrolling = true;
    private int contadorColisionesSuelo = 0;
    private bool enSuelo;

    [Header("Combate")]
    public float attackCooldown = 1.5f;
    public int health = 3;
    public float knockbackForce = 8f;
    private float lastAttackTime;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Configuración para enemigo TERRESTRE
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 2f; // Activar gravedad
        }
        
        startPosition = transform.position;
        
        // Buscar al jugador automáticamente
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
        if (player == null || isDead || isKnockedBack) return;

        // Detectar si está en el suelo
        enSuelo = contadorColisionesSuelo > 0;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si está en rango de ataque
        if (distanceToPlayer <= stopDistance)
        {
            movement = Vector2.zero;
            isPatrolling = false;
            
            if (animator != null)
            {
                animator.SetBool("Attack", true);
            }
        }
        // Si está en rango de detección (perseguir)
        else if (distanceToPlayer < detectionRadius)
        {
            isPatrolling = false;
            
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);

            // Voltear sprite
            if (direction.x < 0)
                transform.localScale = new Vector3(-2, 2, 1);
            else if (direction.x > 0)
                transform.localScale = new Vector3(2, 2, 1);

            if (animator != null)
            {
                animator.SetBool("Attack", false);
            }
        }
        else
        {
            // Patrullar
            isPatrolling = true;
            Patrol();
            
            if (animator != null)
            {
                animator.SetBool("Attack", false);
            }
        }
    }

    void Patrol()
    {
        float distanceFromStart = transform.position.x - startPosition.x;

        // Cambiar dirección en los límites
        if (distanceFromStart >= patrolDistance)
        {
            patrolDirection = -1f;
            transform.localScale = new Vector3(-5, 5, 1);
        }
        else if (distanceFromStart <= -patrolDistance)
        {
            patrolDirection = 1f;
            transform.localScale = new Vector3(5, 5, 1);
        }

        movement = new Vector2(patrolDirection * patrolSpeed, 0);
    }

    void FixedUpdate()
    {
        if (isDead || isKnockedBack) return;
        
        // Mover solo en X, dejar que la gravedad maneje Y
        if (enSuelo)
        {
            if (isPatrolling)
            {
                rb.linearVelocity = new Vector2(movement.x * patrolSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(movement.x * speed, rb.linearVelocity.y);
            }
        }
    }

    // Detectar colisiones con el suelo
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Detectar suelo
        if (collision.contacts[0].normal.y > 0.5f)
        {
            contadorColisionesSuelo++;
        }

        // Detectar jugador y atacar
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Vector2 direccionDanio = transform.position;
                
                Samurai samurai = collision.gameObject.GetComponent<Samurai>();
                if (samurai != null)
                {
                    samurai.RecibeDanio(direccionDanio, 1);
                    lastAttackTime = Time.time;
                    Debug.Log("¡Mushroom atacó al jugador!");
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Detectar cuando deja el suelo
        contadorColisionesSuelo--;
        if (contadorColisionesSuelo < 0) contadorColisionesSuelo = 0;

        // Desactivar daño del jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            Samurai samurai = collision.gameObject.GetComponent<Samurai>();
            if (samurai != null)
            {
                samurai.DesactivaDanio();
            }
            Debug.Log("Mushroom dejó de tocar al jugador");
        }
    }

    // Método sobrecargado para recibir daño
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    public void TakeDamage(int damage, Vector2 direccionGolpe)
    {
        if (isDead) return;

        health -= damage;
        
        if (animator != null)
        {
            animator.SetBool("Hit", true);
            StartCoroutine(ResetHit());
        }

        // Aplicar knockback
        if (rb != null)
        {
            movement = Vector2.zero;
            
            // Calcular dirección del knockback
            float direccionX = transform.position.x - direccionGolpe.x;
            Vector2 direccionKnockback = new Vector2(Mathf.Sign(direccionX), 0.5f).normalized;
            
            // Aplicar fuerza
            rb.linearVelocity = direccionKnockback * knockbackForce;
            
            StartCoroutine(KnockbackRecovery());
            
            Debug.Log($"Mushroom recibió knockback. Dirección: {direccionKnockback}");
        }

        Debug.Log($"Mushroom recibió {damage} de daño. Vida restante: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    IEnumerator KnockbackRecovery()
    {
        isKnockedBack = true;
        float tempSpeed = speed;
        speed = 0;
        
        yield return new WaitForSeconds(0.3f);
        
        speed = tempSpeed;
        isKnockedBack = false;
    }

    IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.3f);
        if (animator != null)
        {
            animator.SetBool("Hit", false);
        }
    }

    void Die()
    {
        isDead = true;
        
        if (animator != null)
        {
            animator.SetBool("Death", true);
            animator.SetBool("Attack", false);
            animator.SetBool("Hit", false);
        }

        // Detener movimiento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        // Desactivar collider y script
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Debug.Log("Mushroom ha muerto");

        Destroy(gameObject, 2f);
    }

    void OnDrawGizmosSelected()
    {
        // Radio de detección (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Distancia de ataque (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        // Área de patrullaje (verde)
        Gizmos.color = Color.green;
        Vector3 patrolStart = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawLine(patrolStart + Vector3.left * patrolDistance, patrolStart + Vector3.right * patrolDistance);
        Gizmos.DrawWireSphere(patrolStart + Vector3.left * patrolDistance, 0.2f);
        Gizmos.DrawWireSphere(patrolStart + Vector3.right * patrolDistance, 0.2f);
    }
    // Método público llamado por MEnemyDZ
    public void AtacarJugador(Collider2D jugador)
    {
        if (isDead) return;

        // Atacar con cooldown
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Vector2 direccionDanio = transform.position;
            
            Samurai samurai = jugador.GetComponent<Samurai>();
            if (samurai != null)
            {
                // Activar animación de ataque
                if (animator != null)
                {
                    animator.SetBool("Attack", true);
                    StartCoroutine(DesactivarAnimacionAtaque());
                }
                
                samurai.RecibeDanio(direccionDanio, 1);
                lastAttackTime = Time.time;
                Debug.Log("¡Mushroom atacó al jugador!");
            }
        }
    }

    IEnumerator DesactivarAnimacionAtaque()
    {
        yield return new WaitForSeconds(0.5f);
        if (animator != null)
        {
            animator.SetBool("Attack", false);
        }
    }
}