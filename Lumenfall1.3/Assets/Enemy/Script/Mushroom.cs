using UnityEngine;
using System.Collections;

public class Mushroom : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;

    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movimiento")]
    public float patrolSpeed = 1.2f;
    public float runSpeed = 2.2f;
    public float patrolDistance = 3f;
    private float patrolDir = 1;
    private Vector3 startPos;

    [Header("Detección")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;

    [Header("Ataque")]
    public float attackCooldown = 1f;
    private float lastAttackTime;

    // Estados internos
    private bool isDead = false;
    private bool isHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        startPos = transform.position;
        currentHealth = maxHealth;

        // Buscar jugador automáticamente
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null || isDead || isHit) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // ------------------- ATAQUE -------------------
        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isWalking", false);

            if (Time.time - lastAttackTime >= attackCooldown)
                DoAttack();

            return;
        }

        // ------------------- PERSEGUIR -------------------
        if (dist <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // ---------------------- PATRULLA ----------------------
    void Patrol()
    {
        anim.SetBool("isWalking", true);

        float distFromStart = transform.position.x - startPos.x;

        if (distFromStart >= patrolDistance)
        {
            patrolDir = -1;
            Flip(-1);
        }
        else if (distFromStart <= -patrolDistance)
        {
            patrolDir = 1;
            Flip(1);
        }

        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, 0);
    }

    // ---------------------- PERSEGUIR ----------------------
    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        Flip(dir);

        rb.linearVelocity = new Vector2(dir * runSpeed, 0);
    }

    // ---------------------- ATAQUE ----------------------
    void DoAttack()
    {
        anim.SetTrigger("Attack");

        lastAttackTime = Time.time;

        // Aplicar daño al samurai
        Samurai samurai = player.GetComponent<Samurai>();
        if (samurai)
            samurai.RecibeDanio(transform.position, 1);
    }

    // ---------------------- RECIBIR DAÑO ----------------------
    public void ForzarAtaque(Collider2D playerCol)
{
    if (isDead) return;

    float dist = Vector2.Distance(transform.position, playerCol.transform.position);

    // Solo ataca si está en rango
    if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
    {
        anim.SetTrigger("Attack");
        lastAttackTime = Time.time;

        Samurai samurai = playerCol.GetComponent<Samurai>();
        if (samurai != null)
        {
            samurai.RecibeDanio(transform.position, 1);
        }
    }
}

    public void TakeDamage(int dmg, Vector2 desde)
    {
        if (isDead) return;

        currentHealth -= dmg;

        anim.SetTrigger("Hit");
        isHit = true;

        // Knockback
        float dir = Mathf.Sign(transform.position.x - desde.x);
        rb.linearVelocity = new Vector2(dir * 4f, 0);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(RecoverHit());
        }
    }

    IEnumerator RecoverHit()
    {
        yield return new WaitForSeconds(0.25f);
        isHit = false;
    }

    // ---------------------- MUERTE ----------------------
    void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Death");

        col.enabled = false;

        Destroy(gameObject, 2f);
    }

    // ---------------------- VOLTEAR ----------------------
    void Flip(float dir)
    {
        transform.localScale = new Vector3(dir > 0 ? 5 : -5, 5, 1); 
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
