using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Samurai : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 9f;
    public float multiplicadorCaida = 2.5f;
    public float multiplicadorSaltoCorto = 2f;

    [Header("Ataque")]
    public float dashFuerza = 7f;
    public SwordHitbox swordHitbox; // Mantener para compatibilidad (opcional)
    
    [Header("Hitboxes de Ataque")]
    public GameObject attack1Hitbox; // ← Arrastra Attack1_Hitbox aquí
    public GameObject attack2Hitbox; // ← Arrastra Attack2_Hitbox aquí
    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    [Header("Vida")]
    public int vidaMaxima = 5;
    private int vidaActual;
    public HealthBar healthBar;

    [Header("Referencias")]
    public Animator animator;
    public Rigidbody2D rb;
    private Collider2D cuerpoCollider;

    [Header("Game Over")]
    public GameOver gameOver;

    [Header("Control de Nivel")]
    public float originalGravityScale = 1f;
    public int disableControlCounter = 0;

    private bool enSuelo;
    private bool recibiendoDanio;
    private bool atacando;
    private bool atacando2;
    private bool estaMuerto;
    private bool puedeMover = true;

    [Header("Habilidades")]
    public bool dobleSaltoHabilitado = false;
    public bool puedeDobleSaltar = false;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    
    [Header("Asistencia de Salto Pro")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private ContactFilter2D groundFilter;
    private readonly Collider2D[] groundHits = new Collider2D[4];

    private Vector3 posicionInicial;
    private Coroutine ataqueActual;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cuerpoCollider = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb != null)
            originalGravityScale = rb.gravityScale;

        posicionInicial = transform.position;

        if (swordHitbox == null)
        {
            swordHitbox = GetComponentInChildren<SwordHitbox>();
            if (swordHitbox == null)
                Debug.LogWarning("No se encontró SwordHitbox (sistema antiguo)");
        }

        if (gameOver == null)
        {
            if (gameOver == null)
                Debug.LogWarning("GameOver no encontrado en la escena.");
        }

        // Configurar filtro de suelo
        groundFilter.useLayerMask = true;
        groundFilter.layerMask = groundLayer;
        groundFilter.useNormalAngle = true;
        groundFilter.minNormalAngle = 80f;
        groundFilter.maxNormalAngle = 100f;
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("⚠️ Samurai NO detecta GameManager, usando valores locales");
            vidaActual = vidaMaxima;
        }
        else
        {
            vidaMaxima = GameManager.Instance.vidaMaximaJugador;
            vidaActual = GameManager.Instance.vidaActualJugador;
            dobleSaltoHabilitado = GameManager.Instance.habilidadDobleSalto;
        }

        if (healthBar == null)
            healthBar = FindFirstObjectByType<HealthBar>();

        if (swordHitbox != null)
            swordHitbox.DesactivarHitbox();
        
        // ✅ Asegurar que las hitboxes estén desactivadas al inicio
        if (attack1Hitbox != null)
            attack1Hitbox.SetActive(false);
        if (attack2Hitbox != null)
            attack2Hitbox.SetActive(false);
    }

    void Update()
    {
        if (estaMuerto) return;

        DetectarSueloReal();
        animator.SetBool("ensuelo", enSuelo);

        // Coyote Time
        if (enSuelo)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (puedeMover && disableControlCounter <= 0)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float movimiento = inputX * velocidad;
            animator.SetFloat("Movement", Mathf.Abs(inputX));

            if (inputX < 0) transform.localScale = new Vector3(-2, 2, 1);
            else if (inputX > 0) transform.localScale = new Vector3(2, 2, 1);

            rb.linearVelocity = new Vector2(movimiento, rb.linearVelocity.y);

            // Sistema de salto
            if (jumpBufferCounter > 0)
            {
                if (coyoteTimeCounter > 0)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
                    jumpBufferCounter = 0;
                    puedeDobleSaltar = dobleSaltoHabilitado;
                }
                else if (puedeDobleSaltar)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
                    puedeDobleSaltar = false;
                    jumpBufferCounter = 0;
                }
            }

            // ✅ ATAQUE 1 (J) - Usa Attack1_Hitbox
            if (Input.GetKeyDown(KeyCode.J) && !atacando && !atacando2 && Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                atacando = true;
                animator.SetTrigger("Ataque");
                ataqueActual = StartCoroutine(FinAtaque1());
            }

            // ✅ ATAQUE 2 (K) - Usa Attack2_Hitbox
            if (Input.GetKeyDown(KeyCode.K) && !atacando && !atacando2 && Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                atacando2 = true;
                animator.SetTrigger("Ataque2");
                DashLigero();
                ataqueActual = StartCoroutine(FinAtaque2());
            }
        }
        else
        {
            animator.SetFloat("Movement", 0);
        }
    }

    void DetectarSueloReal()
    {
        enSuelo = false;

        ContactPoint2D[] contactos = new ContactPoint2D[8];
        int cantidad = cuerpoCollider.GetContacts(contactos);

        for (int i = 0; i < cantidad; i++)
        {
            ContactPoint2D c = contactos[i];

            if (((1 << c.collider.gameObject.layer) & groundLayer) == 0)
                continue;

            if (c.normal.y > 0.6f)
            {
                enSuelo = true;
                puedeDobleSaltar = dobleSaltoHabilitado;
                return;
            }
        }
    }

    void DashLigero()
    {
        float direccion = transform.localScale.x;
        rb.linearVelocity = new Vector2(direccion * dashFuerza, rb.linearVelocity.y);
    }

   // ✅ Corrutina para Ataque 1
IEnumerator FinAtaque1()
{
    // Esperar al frame exacto del golpe
    yield return new WaitForSeconds(0.2f); // ← Ajusta este valor

    if (attack1Hitbox != null)
    {
        attack1Hitbox.SetActive(true);
        Debug.Log("🗡️ Attack1_Hitbox activada");
    }

    // Hitbox activa solo por un momento corto
    yield return new WaitForSeconds(0.1f); // ← Muy corto para contacto preciso

    if (attack1Hitbox != null)
    {
        attack1Hitbox.SetActive(false);
        Debug.Log("🗡️ Attack1_Hitbox desactivada");
    }

    atacando = false;
}

// ✅ Corrutina para Ataque 2
IEnumerator FinAtaque2()
{
    // Esperar al frame exacto del golpe
    yield return new WaitForSeconds(0.25f); // ← Ajusta este valor

    if (attack2Hitbox != null)
    {
        attack2Hitbox.SetActive(true);
        Debug.Log("⚔️ Attack2_Hitbox activada");
    }

    // Hitbox activa solo por un momento corto
    yield return new WaitForSeconds(0.1f); // ← Muy corto

    if (attack2Hitbox != null)
    {
        attack2Hitbox.SetActive(false);
        Debug.Log("⚔️ Attack2_Hitbox desactivada");
    }

    atacando2 = false;
}

    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && !estaMuerto)
        {
            vidaActual -= cantDanio;

            if (GameManager.Instance != null)
                GameManager.Instance.vidaActualJugador = vidaActual;

            if (healthBar != null)
                healthBar.AnimarDanio();

            if (vidaActual <= 0)
            {
                vidaActual = 0;
                Morir();
                return;
            }

            StartCoroutine(HurtRutina());
            Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized;
            rb.AddForce(rebote * 5f, ForceMode2D.Impulse);
        }
    }

    public void DesactivaDanio()
    {
        recibiendoDanio = false;
    }

    IEnumerator HurtRutina()
    {
        if (estaMuerto) yield break;

        recibiendoDanio = true;
        puedeMover = false;
        animator.SetTrigger("Hurt");

        yield return new WaitForSeconds(0.2f);

        recibiendoDanio = false;
        puedeMover = true;
    }

    public void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        puedeMover = false;

        if (ataqueActual != null)
        {
            StopCoroutine(ataqueActual);
            if (swordHitbox != null)
                swordHitbox.DesactivarHitbox();
            if (attack1Hitbox != null)
                attack1Hitbox.SetActive(false);
            if (attack2Hitbox != null)
                attack2Hitbox.SetActive(false);
        }

        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        if (gameOver != null)
            gameOver.MostrarGameOver();
        else
            Debug.LogWarning("gameOver es null — asigna el GameOver en el Inspector.");

        StartCoroutine(RespawnDespues(2f));
    }

    IEnumerator RespawnDespues(float t)
    {
        yield return new WaitForSeconds(t);

        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        transform.position = posicionInicial;
        estaMuerto = false;
        puedeMover = true;
        atacando = false;
        atacando2 = false;

        vidaActual = vidaMaxima;

        if (GameManager.Instance != null)
            GameManager.Instance.vidaActualJugador = vidaActual;

        animator.Play("Idle");
    }

    public void Curar(int cantidad)
    {
        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMaxima);

        if (GameManager.Instance != null)
            GameManager.Instance.vidaActualJugador = vidaActual;
    }

    public int ObtenerVidaActual()
    {
        return vidaActual;
    }

    public int ObtenerVidaMaxima()
    {
        return vidaMaxima;
    }

    void OnDrawGizmos()
    {
        if (cuerpoCollider == null) return;

        Gizmos.color = enSuelo ? Color.green : Color.red;
        Gizmos.DrawWireCube(cuerpoCollider.bounds.center, cuerpoCollider.bounds.size);
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplicadorSaltoCorto - 1) * Time.fixedDeltaTime;
        }
    }
}