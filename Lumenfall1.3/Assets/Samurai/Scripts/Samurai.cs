using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Samurai : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    public float longitudRaycast = 0.1f;
    public LayerMask capaSuelo;

    [Header("Ataque")]
    public float dashFuerza = 7f;
    public SwordHitbox swordHitbox; // Referencia a la hitbox de la espada

    [Header("Vida")]
    public int vidaMaxima = 3;
    private int vidaActual;
    public HealthBar healthBar; // NUEVO: Referencia a la barra de vida para animaciones

    [Header("Referencias")]
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Control de Nivel")]
    public float originalGravityScale = 1f;
    public int disableControlCounter = 0;

    private bool enSuelo;
    private bool recibiendoDanio;
    private bool atacando;
    private bool atacando2;
    private bool estaMuerto;
    private bool puedeMover = true;

    private Vector3 posicionInicial;
    private Coroutine ataqueActual; // Guardar referencia a la corrutina de ataque

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb != null)
            originalGravityScale = rb.gravityScale;

        posicionInicial = transform.position;

        // Buscar automáticamente la hitbox de la espada
        if (swordHitbox == null)
        {
            swordHitbox = GetComponentInChildren<SwordHitbox>();
            if (swordHitbox == null)
            {
                Debug.LogWarning("No se encontró SwordHitbox en los hijos del Samurai");
            }
        }
    }

    void Start()
    {
        // Inicializar vida
        vidaActual = vidaMaxima;
        Debug.Log($"Samurai inició con {vidaActual} puntos de vida");

        // Buscar automáticamente la barra de vida si no está asignada
        if (healthBar == null)
        {
            healthBar = FindFirstObjectByType<HealthBar>();
        }

        // Asegurarse de que la hitbox esté desactivada al inicio
        if (swordHitbox != null)
        {
            swordHitbox.DesactivarHitbox();
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        Vector2 origenRaycast = new Vector2(transform.position.x, transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origenRaycast, Vector2.down, longitudRaycast, capaSuelo);
        enSuelo = hit.collider != null;
        animator.SetBool("ensuelo", enSuelo);

        // IMPORTANTE: Solo leer input si puede moverse
        if (puedeMover && disableControlCounter <= 0)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float movimiento = inputX * velocidad * Time.deltaTime;

            animator.SetFloat("Movement", Mathf.Abs(inputX));

            if (inputX < 0) transform.localScale = new Vector3(-2, 2, 1);
            else if (inputX > 0) transform.localScale = new Vector3(2, 2, 1);

            transform.position += new Vector3(movimiento, 0, 0);

            if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            }

            if (Input.GetKeyDown(KeyCode.J) && !atacando && !atacando2)
            {
                atacando = true;
                animator.SetTrigger("Ataque");
                ataqueActual = StartCoroutine(FinAtaque(0.5f, 1));
            }

            if (Input.GetKeyDown(KeyCode.K) && !atacando && !atacando2)
            {
                atacando2 = true;
                animator.SetTrigger("Ataque2");
                DashLigero();
                ataqueActual = StartCoroutine(FinAtaque(0.9f, 2));
            }
        }
        else
        {
            // Si no puede moverse, detener animación
            animator.SetFloat("Movement", 0);
        }
    }
    
    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && !estaMuerto)
        {
            // Reducir vida
            vidaActual -= cantDanio;
            Debug.Log($"💔 Samurai recibió {cantDanio} de daño. Vida restante: {vidaActual}/{vidaMaxima}");

            // Animar la barra de vida si existe
            if (healthBar != null)
            {
                healthBar.AnimarDanio();
            }

            // Verificar si murió
            if (vidaActual <= 0)
            {
                vidaActual = 0; // Asegurar que no sea negativa
                Morir();
                return;
            }

            // Si no murió, aplicar daño normal (pero NO cancelar ataque)
            StartCoroutine(HurtRutina());
            Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized;
            rb.AddForce(rebote * 5f, ForceMode2D.Impulse);
        }
    }

    public void DesactivaDanio()
    {
        recibiendoDanio = false;
        puedeMover = true;
    }

    void DashLigero()
    {
        float direccion = transform.localScale.x;
        rb.linearVelocity = new Vector2(direccion * dashFuerza, rb.linearVelocity.y);
    }

    // Gestiona la hitbox durante el ataque - NO se detiene aunque recibas daño
    IEnumerator FinAtaque(float t, int tipoAtaque)
    {
        // Activar hitbox al inicio del ataque
        if (swordHitbox != null)
        {
            int danio = (tipoAtaque == 2) ? 2 : 1;
            swordHitbox.ActivarHitbox(danio);
        }

        // Esperar tiempo del ataque
        yield return new WaitForSeconds(t);

        // Desactivar hitbox al terminar
        if (swordHitbox != null)
        {
            swordHitbox.DesactivarHitbox();
        }

        atacando = false;
        atacando2 = false;
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

        // Detener cualquier ataque en progreso
        if (ataqueActual != null)
        {
            StopCoroutine(ataqueActual);
            if (swordHitbox != null)
            {
                swordHitbox.DesactivarHitbox();
            }
        }

        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

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
        
        // Restaurar vida completa
        vidaActual = vidaMaxima;
        Debug.Log($"Samurai respawneó con {vidaActual} puntos de vida");
        
        animator.Play("Idle");
    }

    // Métodos públicos para gestionar vida (para UI, power-ups, etc.)
    public void Curar(int cantidad)
    {
        vidaActual = Mathf.Min(vidaActual + cantidad, vidaMaxima);
        Debug.Log($"Samurai curado. Vida actual: {vidaActual}/{vidaMaxima}");
    }

    public int ObtenerVidaActual()
    {
        return vidaActual;
    }

    public int ObtenerVidaMaxima()
    {
        return vidaMaxima;
    }
}