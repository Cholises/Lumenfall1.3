using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Samurai : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 9f;   // altura real del salto
    public float multiplicadorCaida = 2.5f; // qué tan rápido cae
    public float multiplicadorSaltoCorto = 2f; // para salto corto

    [Header("Ataque")]
    public float dashFuerza = 7f;
    public SwordHitbox swordHitbox;

    [Header("Vida")]
    public int vidaMaxima = 5;
    private int vidaActual;
    public HealthBar healthBar;

    [Header("Referencias")]
    public Animator animator;
    public Rigidbody2D rb;
    private Collider2D cuerpoCollider;

    [Header("Game Over")]
    public GameOver gameOver; // ← AGREGADO: arrastra aquí tu objeto GameOver en el Inspector

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
    public float coyoteTime = 0.15f;     // tiempo para saltar después de caer
    public float jumpBufferTime = 0.15f; // tiempo para guardar el salto antes de tocar suelo

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // 🔴 FILTRO REAL PARA SOLO SUELO POR ABAJO
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
                Debug.LogWarning("No se encontró SwordHitbox en los hijos del Samurai");
        }

        // Intentar asignar la referencia al GameOver automáticamente si no la asignaron en el inspector.
        if (gameOver == null)
        {
    
            if (gameOver == null)
                Debug.LogWarning("GameOver no encontrado en la escena. Arrastra el GameOver al campo 'gameOver' del Samurai.");
        }

        // ✅ CONFIGURAR FILTRO PARA DETECTAR SOLO SUELO DESDE ABAJO
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
    }

    void Update()
    {
        if (estaMuerto) return;

        DetectarSueloReal();
        animator.SetBool("ensuelo", enSuelo);

        // ✅ COYOTE TIME
        if (enSuelo)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // ✅ JUMP BUFFER
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

            // ✅ ÚNICO BLOQUE DE SALTO (BORRA EL TUYO VIEJO)
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

            if (Input.GetKeyDown(KeyCode.J) && !atacando && !atacando2)
            {
                atacando = true;
                animator.SetTrigger("Ataque");
                ataqueActual = StartCoroutine(FinAtaque(0.35f, 1));
            }

            if (Input.GetKeyDown(KeyCode.K) && !atacando && !atacando2)
            {
                atacando2 = true;
                animator.SetTrigger("Ataque2");
                DashLigero();
                ataqueActual = StartCoroutine(FinAtaque(0.6f, 2));
            }
        }
        else
        {
            animator.SetFloat("Movement", 0);
        }
    }

    // ✅ DETECCIÓN REAL SOLO DESDE ABAJO (NO PAREDES / NO TECHO)
    void DetectarSueloReal()
    {
        enSuelo = false;

        ContactPoint2D[] contactos = new ContactPoint2D[8];
        int cantidad = cuerpoCollider.GetContacts(contactos);

        for (int i = 0; i < cantidad; i++)
        {
            ContactPoint2D c = contactos[i];

            // ✅ 1. Debe ser del layer Ground
            if (((1 << c.collider.gameObject.layer) & groundLayer) == 0)
                continue;

            // ✅ 2. La normal debe apuntar HACIA ARRIBA (suelo real)
            // Esto descarta paredes y techos
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

    IEnumerator FinAtaque(float t, int tipoAtaque)
    {
        yield return new WaitForSeconds(0.05f);

        if (swordHitbox != null)
            swordHitbox.ActivarHitbox(tipoAtaque);

        yield return new WaitForSeconds(t);

        if (swordHitbox != null)
            swordHitbox.DesactivarHitbox();

        atacando = false;
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
        }

        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Mostramos el Game Over (si está asignado)
        if (gameOver != null)
            gameOver.MostrarGameOver();
        else
            Debug.LogWarning("gameOver es null en Samurai.Morir() — asigna el GameOver en el Inspector o asegúrate de que exista un GameOver en la escena.");

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
        // Caída más rápida
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        }
        // Salto corto si sueltas rápido el botón
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (multiplicadorSaltoCorto - 1) * Time.fixedDeltaTime;
        }
    }
}
