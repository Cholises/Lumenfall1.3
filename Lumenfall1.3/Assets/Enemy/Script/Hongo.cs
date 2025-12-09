using UnityEngine;
using System.Collections;

public class Hongo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;

    [Header("Damage Zone")]
    public HongoDamageZone damageZone;   // ← AGREGADO

    [Header("Stats")]
    public int maxHealth = 4;
    private int currentHealth;

    [Header("Movimiento")]
    public float patrolSpeed = 1.3f;
    public float runSpeed = 2.5f;
    public float patrolDistance = 2.5f;
    private float patrolDir = 1;
    private Vector3 startPos;

    [Header("Detección")]
    public float detectionRange = 3f;
    public float attackRange = 1.1f;

    [Header("Ataques")]
    public float attackCooldown = 1f;
    private float lastAttackTime;
    private bool nextStunAttack = false;

    private bool isDead = false;
    private bool isHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 3;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        startPos = transform.position;
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // La zona de daño inicia apagada
        if (damageZone != null)
            damageZone.DisableDamage();
    }

    void Update()
    {
        if (isDead || isHit || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // ATAQUE
        if (dist <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isWalking", false);
            DoAttack();
            return;
        }

        // PERSEGUIR
        if (dist <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("isWalking", true);

        float distX = transform.position.x - startPos.x;

        if (distX >= patrolDistance)
        {
            patrolDir = -1;
            Flip(-1);
        }
        else if (distX <= -patrolDistance)
        {
            patrolDir = 1;
            Flip(1);
        }

        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, 0);
    }

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

        lastAttackTime = Time.time;

        // Alterna ataque normal y stun
        if (!nextStunAttack)
        {
            anim.SetTrigger("Attack");
            nextStunAttack = true;
        }
        else
        {
            anim.SetTrigger("AttackStun");
            nextStunAttack = false;
        }

        // Aplicar daño al samurai
        Samurai samurai = player.GetComponent<Samurai>();
        if (samurai)
            samurai.RecibeDanio(transform.position, 1);
    }

    public void RealizarDaño(Collider2D target)
{
    Samurai sam = target.GetComponent<Samurai>();

    if (sam != null)
    {
        // EL MÉTODO REAL DE TU PERSONAJE ES ESTE:
        sam.RecibeDanio(transform.position, 1);
    }
}
    // ----------- DAMAGEZONE EVENTS -----------
    public void EnableDamage()   // ← Se llama desde el Animator
    {
        if (!isDead && damageZone != null)
            damageZone.EnableDamage();
    }

    public void DisableDamage()  // ← Se llama desde el Animator
    {
        if (damageZone != null)
            damageZone.DisableDamage();
    }
    // ----------------------------------------

    public void TakeDamage(int dmg, Vector2 desde)
    {
        if (isDead) return;

        currentHealth -= dmg;

        anim.SetTrigger("Hit");
        isHit = true;

        float dir = Mathf.Sign(transform.position.x - desde.x);
        rb.linearVelocity = new Vector2(dir * 3.5f, 0);

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

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Death");
        col.enabled = false;
        Destroy(gameObject, 2f);
    }

    // 👉 Flip CORREGIDO (tu sprite original mira a la izquierda)
    void Flip(float dir)
    {
        transform.localScale = new Vector3(dir > 0 ? -5 : 5, 5, 1);
    }
}
