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
    public float stopDistance = 1.5f;
    public float patrolSpeed = 1.5f; // Velocidad de patrullaje
    public float patrolDistance = 3f; // Distancia que recorre en cada dirección
    private Vector2 movement;
    private bool isKnockedBack = false;
    
    // Variables de patrullaje
    private Vector3 startPosition;
    private float patrolDirection = 1f; // 1 = derecha, -1 = izquierda
    private bool isPatrolling = true;

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
        
        // IMPORTANTE: Congelar el eje Y para que no caiga y desactivar gravedad
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
            rb.gravityScale = 0; // Desactivar gravedad completamente
        }
        
        // Guardar posición inicial para patrullaje
        startPosition = transform.position;
        
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
        if (player == null || isDead || isKnockedBack) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Si está en rango de ataque (cerca del jugador)
        if (distanceToPlayer <= stopDistance)
        {
            movement = Vector2.zero;
            isPatrolling = false;
            
            // Activar animación de ataque
            if (animator != null)
            {
                animator.SetBool("Attack", true);
            }
        }
        // Si está en rango de detección pero lejos (perseguir al jugador)
        else if (distanceToPlayer < detectionRadius)
        {
            isPatrolling = false;
            
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
            // Fuera de rango - patrullar
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
        // Calcular la distancia desde la posición inicial
        float distanceFromStart = transform.position.x - startPosition.x;

        // Cambiar dirección si alcanza el límite de patrullaje
        if (distanceFromStart >= patrolDistance)
        {
            patrolDirection = -1f; // Ir a la izquierda
            transform.localScale = new Vector3(-2, 2, 1); // Voltear sprite
        }
        else if (distanceFromStart <= -patrolDistance)
        {
            patrolDirection = 1f; // Ir a la derecha
            transform.localScale = new Vector3(2, 2, 1); // Voltear sprite
        }

        // Mover en la dirección de patrullaje
        movement = new Vector2(patrolDirection * patrolSpeed, 0);
    }

    void FixedUpdate()
    {
        if (isDead || isKnockedBack) return;
        
        // Mover en FixedUpdate para mejor física
        if (isPatrolling)
        {
            // Usar velocidad de patrullaje
            rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
        }
        else
        {
            // Usar velocidad normal para perseguir
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        }
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
                // Activar animación de ataque
                if (animator != null)
                {
                    animator.SetBool("Attack", true);
                    StartCoroutine(DesactivarAnimacionAtaque());
                }
                
                samurai.RecibeDanio(direccionDanio, 1);
                lastAttackTime = Time.time;
                Debug.Log("¡Enemigo atacó al jugador!");
            }
        }
    }

    IEnumerator DesactivarAnimacionAtaque()
    {
        yield return new WaitForSeconds(0.5f); // Duración de la animación de ataque
        if (animator != null)
        {
            animator.SetBool("Attack", false);
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

        // Aplicar knockback (empuje hacia atrás) SOLO EN EL EJE X
        if (rb != null)
        {
            // Detener el movimiento actual
            movement = Vector2.zero;
            
            // Calcular dirección opuesta al golpe (SOLO horizontal para voladores)
            float direccionX = transform.position.x - direccionGolpe.x;
            Vector2 direccionKnockback = new Vector2(Mathf.Sign(direccionX), 0).normalized; // Y = 0 para mantenerlo en el aire
            
            // Aplicar fuerza de empuje más fuerte SOLO EN X
            rb.linearVelocity = new Vector2(direccionKnockback.x * knockbackForce, 0);
            
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
        rb.linearVelocity = new Vector2(0, 0); // Detener completamente SOLO en X después del knockback
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

        // DESCONGELA el eje Y y reactiva la gravedad para que caiga
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Solo mantener rotación congelada
            rb.gravityScale = 2f; // Activar gravedad para que caiga
            rb.linearVelocity = new Vector2(0, 0); // Detener movimiento horizontal
        }

        // Desactivar collider y script
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;

        Debug.Log("Enemigo ha muerto - cayendo");

        // Destruir después de caer y animación
        Destroy(gameObject, 3f); // Más tiempo para que caiga
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
 

private void OnCollisionStay2D(Collision2D collision)
{
    if (isDead) return;

    if (collision.gameObject.CompareTag("Player"))
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Samurai samurai = collision.gameObject.GetComponent<Samurai>();
            if (samurai != null)
            {
                samurai.RecibeDanio(transform.position, 1);
                lastAttackTime = Time.time;
            }
        }
    }
}
}