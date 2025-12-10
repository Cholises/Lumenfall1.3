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
    public HongoDamageZone damageZone;

    [Header("Stats")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Movimiento")]
    public float patrolSpeed = 1.3f;
    public float runSpeed = 2.5f;
    public float patrolDistance = 2.5f;
    private float patrolDir = 1;
    private Vector3 startPos;
    private bool facingRight = true;

    [Header("Detección")]
    public float detectionRange = 3f;
    public float attackRange = 1.1f;

    [Header("Ataques")]
    public float attackCooldown = 1f;
    private float lastAttackTime;

    private bool isDead = false;
    private bool isHit = false;
    private bool isAttacking = false;

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

        if (damageZone != null)
            damageZone.DisableDamage();
    }

    void Update()
    {
        if (isDead || isHit || isAttacking || player == null) return;

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

    void LateUpdate()
    {
        // Evitar que se encoja por las animaciones
        if (!isDead)
        {
            transform.localScale = new Vector3(facingRight ? -5 : 5, 5, 5);
        }
    }

    void Patrol()
    {
        anim.SetBool("isWalking", true);

        float distX = transform.position.x - startPos.x;

        if (distX >= patrolDistance)
        {
            patrolDir = -1;
            Flip(false);
        }
        else if (distX <= -patrolDistance)
        {
            patrolDir = 1;
            Flip(true);
        }

        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, rb.linearVelocity.y);
    }

    void ChasePlayer()
    {
        anim.SetBool("isWalking", true);

        float dir = Mathf.Sign(player.position.x - transform.position.x);
        Flip(dir > 0);

        rb.linearVelocity = new Vector2(dir * runSpeed, rb.linearVelocity.y);
    }

    // ---------------------- ATAQUE ----------------------
    void DoAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        lastAttackTime = Time.time;

        anim.SetBool("Attack", true);

        StartCoroutine(AttackFinish());
    }

    IEnumerator AttackFinish()
    {
        yield return new WaitForSeconds(0.6f);

        anim.SetBool("Attack", false);
        isAttacking = false;
        DisableDamage();
    }

    // ESTOS MÉTODOS SE LLAMAN DESDE ANIMATION EVENTS
    public void EnableDamage()
    {
        Debug.Log("Hongo: EnableDamage llamado");
        if (!isDead && damageZone != null)
            damageZone.EnableDamage();
    }

    public void DisableDamage()
    {
        if (damageZone != null)
            damageZone.DisableDamage();
    }

    public void RealizarDaño(Collider2D target)
    {
        Samurai sam = target.GetComponent<Samurai>();

        Debug.Log("Hongo: Intento de daño al samurai");

        if (sam != null)
        {
            Debug.Log("Hongo: Daño aplicado al samurai");
            sam.RecibeDanio(transform.position, 1);
        }
    }

    // ---------------------- RECIBIR DAÑO ----------------------
    public void TakeDamage(int dmg, Vector2 desde)
    {
        if (isDead) return;

        currentHealth -= dmg;

        anim.SetTrigger("Hit");
        isHit = true;
        isAttacking = false;
        DisableDamage();

        float dir = Mathf.Sign(transform.position.x - desde.x);
        rb.linearVelocity = new Vector2(dir * 3.5f, rb.linearVelocity.y);

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
        anim.SetTrigger("Death");
        
        // Hacer que el Rigidbody sea kinematic para que no caiga
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        
        col.enabled = false;
        DisableDamage();

        Destroy(gameObject, 2f);
    }

    void Flip(bool faceRight)
    {
        facingRight = faceRight;
    }
}