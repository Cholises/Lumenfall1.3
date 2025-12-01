using UnityEngine;
using System.Collections;

public class Mushroom : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;
    public float speed = 2f;
    public float patrolSpeed = 1.5f;
    public float patrolDistance = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0;

    [Header("Interno")]
    private bool Death = false;
    private bool Hit = false;
    private Vector3 startPos;
    private int patrolDir = 1;
    private bool playerInsideZone = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        startPos = transform.position;

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Buscar al jugador si no está asignado
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (Death || Hit) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (playerInsideZone)
        {
            if (dist <= attackRange)
            {
                Attack();
            }
            else if (dist <= detectionRange)
            {
                ChasePlayer();
            }
            else
            {
                playerInsideZone = false;
                Patrol();
            }
        }
        else
        {
            Patrol();

            if (dist <= detectionRange)
                playerInsideZone = true;
        }
    }

    // ---------------------- PATRULLA ----------------------
    void Patrol()
    {
        anim.SetBool("Attack", false);
        anim.SetBool("isWalking", true);

        float distanceFromStart = transform.position.x - startPos.x;

        if (distanceFromStart >= patrolDistance)
        {
            patrolDir = -1;
            Flip(-1);
        }
        else if (distanceFromStart <= -patrolDistance)
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
        anim.SetBool("Attack", false);

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        Flip(dir);

        rb.linearVelocity = new Vector2(dir * speed, 0);
    }

    // ---------------------- ATAQUE ----------------------
    void Attack()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isWalking", false);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            anim.SetTrigger("Attack");

            lastAttackTime = Time.time;

            Samurai samurai = player.GetComponent<Samurai>();
            if (samurai != null)
            {
                samurai.RecibeDanio(transform.position, 1);
            }
        }
    }

    // ---------------------- RECIBIR DAÑO ----------------------
    public void RecibirGolpe(int dmg, Vector2 desde)
    {
        if (Death) return;

        currentHealth -= dmg;
        Hit = true;

        anim.SetTrigger("Hit");

        float dir = Mathf.Sign(transform.position.x - desde.x);
        rb.linearVelocity = new Vector2(dir * 4f, 0);

        if (currentHealth <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(RecuperarHit());
        }
    }

    IEnumerator RecuperarHit()
    {
        yield return new WaitForSeconds(0.25f);
        Hit = false;
    }

    // ---------------------- COMPATIBLE CON SWORDHITBOX ----------------------
    public void TakeDamage(int damage, Vector2 desde)
    {
        RecibirGolpe(damage, desde);
    }

    // ---------------------- MUERTE ----------------------
    void Morir()
    {
        Death = true;

        rb.linearVelocity = Vector2.zero;

        anim.SetBool("isWalking", false);
        anim.SetBool("Attack", false);
        anim.SetTrigger("Death");

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 2f);
    }

    // ---------------------- EXTRA ----------------------
    void Flip(float dir)
    {
        // Escala correcta que tú usas: (5, 5, 1)
        transform.localScale = new Vector3(dir > 0 ? 5 : -5, 5, 1);
    }

    public void EntrarZona() => playerInsideZone = true;
    public void SalirZona() => playerInsideZone = false;
}