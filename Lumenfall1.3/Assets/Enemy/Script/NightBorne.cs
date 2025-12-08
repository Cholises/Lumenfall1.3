using UnityEngine;
using System.Collections;

public class NightBorne : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;
    private SpriteRenderer sprite;

    [Header("Stats")]
    public int maxHealth = 15;
    private int currentHealth;

    [Header("Movimiento")]
    public float runSpeed = 3f;

    [Header("Detección")]
    public float detectionRange = 7f;
    public float attackRange = 2.2f;

    [Header("Ataque")]
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private bool isDead = false;
    private bool isHit = false;
    private bool combatMode = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        transform.localScale = new Vector3(3f, 3f, 1f);
    }

    void Update()
    {
        if (!combatMode)
        {
            Idle();
            return;
        }

        if (player == null || isDead || isHit) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("isRunning", false);

            if (Time.time - lastAttackTime >= attackCooldown)
                StartCoroutine(PerformAttack());

            return;
        }

        if (dist <= detectionRange)
            ChasePlayer();
        else
            Idle();
    }

    public void ActivarCombate()
    {
        combatMode = true;
        Debug.Log("[NightBorne] Combate activado");
    }

    void Idle()
    {
        anim.SetBool("isRunning", false);
        rb.linearVelocity = Vector2.zero;
    }

    void ChasePlayer()
    {
        anim.SetBool("isRunning", true);

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        sprite.flipX = dir < 0;

        rb.linearVelocity = new Vector2(runSpeed * dir, 0);
    }

    IEnumerator PerformAttack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.35f);

        if (player == null) yield break;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 0.5f)
        {
            Samurai s = player.GetComponent<Samurai>();
            if (s != null)
                s.RecibeDanio(transform.position, 2);
        }
    }

    public void TakeDamage(int dmg, Vector2 desde)
    {
        if (isDead) return;

        combatMode = true;

        currentHealth -= dmg;

        // 🔥 Mostrar daño en consola
        Debug.Log("[NightBorne] Daño recibido: " + dmg +
                  " | Vida restante: " + currentHealth + "/" + maxHealth);

        // 🔥 Activar animación de Hurt
        anim.SetTrigger("Hit");
        isHit = true;

        float dir = Mathf.Sign(transform.position.x - desde.x);
        rb.linearVelocity = new Vector2(dir * 3f, 0);

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(RecoverHit());
    }

    IEnumerator RecoverHit()
    {
        yield return new WaitForSeconds(0.35f);
        isHit = false;
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Death");

        col.enabled = false;

        Destroy(gameObject, 3f);
    }
}
