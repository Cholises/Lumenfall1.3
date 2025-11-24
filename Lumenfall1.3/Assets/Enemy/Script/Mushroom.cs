using UnityEngine;
using System.Collections;

public class Mushroom : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sprite;

    [Header("Movimiento")]
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;
    public float stopDistance = 1.5f;
    public float patrolSpeed = 1.5f;
    public float patrolDistance = 3f;

    private Vector2 movement;
    private bool isKnockedBack = false;
    private bool isPatrolling = true;
    private bool facingRight = true;
    private float patrolDirection = 1f;
    private Vector3 startPosition;

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
        sprite = GetComponent<SpriteRenderer>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        rb.gravityScale = 0;

        startPosition = transform.position;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || isDead || isKnockedBack) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- ATAQUE ---
        if (distance <= stopDistance)
        {
            isPatrolling = false;
            movement = Vector2.zero;

            animator.SetBool("Attack", true);
            Flip(player.position.x > transform.position.x);
            return;
        }

        // --- PERSECUCIÓN ---
        if (distance < detectionRadius)
        {
            isPatrolling = false;
            animator.SetBool("Attack", false);

            Vector2 dir = (player.position - transform.position).normalized;
            movement = new Vector2(dir.x, 0);

            Flip(dir.x > 0);
            return;
        }

        // --- PATRULLA ---
        isPatrolling = true;
        animator.SetBool("Attack", false);
        Patrol();
    }

    void Flip(bool faceRight)
    {
        if (facingRight != faceRight)
        {
            facingRight = faceRight;
            sprite.flipX = !faceRight;
        }
    }

    void Patrol()
    {
        float dist = transform.position.x - startPosition.x;

        if (dist >= patrolDistance)
        {
            patrolDirection = -1;
            Flip(false);
        }
        else if (dist <= -patrolDistance)
        {
            patrolDirection = 1;
            Flip(true);
        }

        movement = new Vector2(patrolDirection * patrolSpeed, 0);
    }

    void FixedUpdate()
    {
        if (isDead || isKnockedBack) return;

        float currentSpeed = isPatrolling ? patrolSpeed : speed;

        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    public void AtacarJugador(Collider2D jugador)
    {
        if (Time.time - lastAttackTime < attackCooldown || isDead)
            return;

        Samurai samurai = jugador.GetComponent<Samurai>();
        if (samurai != null)
        {
            samurai.RecibeDanio(transform.position, 1);
            lastAttackTime = Time.time;
        }
    }
}