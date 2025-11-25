using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Samurai : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 15f;

    [Header("Ataque")]
    public float dashFuerza = 7f;
    public SwordHitbox swordHitbox;

    [Header("Vida")]
    public int vidaMaxima = 3;
    private int vidaActual;
    public HealthBar healthBar;

    [Header("Referencias")]
    public Animator animator;
    public Rigidbody2D rb;

    [Header("Control de Nivel")]
    public float originalGravityScale = 1f;
    public int disableControlCounter = 0;

    private int contadorColisionesSuelo = 0; // NUEVO: Contar colisiones con suelo
    private bool enSuelo;
    private bool recibiendoDanio;
    private bool atacando;
    private bool atacando2;
    private bool estaMuerto;
    private bool puedeMover = true;

    private Vector3 posicionInicial;
    private Coroutine ataqueActual;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb != null)
            originalGravityScale = rb.gravityScale;

        posicionInicial = transform.position;

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
        vidaActual = vidaMaxima;
        Debug.Log($"Samurai inició con {vidaActual} puntos de vida");

        if (healthBar == null)
        {
            healthBar = FindFirstObjectByType<HealthBar>();
        }

        if (swordHitbox != null)
        {
            swordHitbox.DesactivarHitbox();
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        // Actualizar estado de suelo basado en colisiones
        enSuelo = contadorColisionesSuelo > 0;
        animator.SetBool("ensuelo", enSuelo);

        if (puedeMover && disableControlCounter <= 0)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float movimiento = inputX * velocidad * Time.deltaTime;

            animator.SetFloat("Movement", Mathf.Abs(inputX));

            if (inputX < 0) transform.localScale = new Vector3(-2, 2, 1);
            else if (inputX > 0) transform.localScale = new Vector3(2, 2, 1);

            transform.position += new Vector3(movimiento, 0, 0);

            // SALTO: Solo cuando está tocando el suelo (colisión activa)
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
            animator.SetFloat("Movement", 0);
        }
    }

    // NUEVO: Detectar cuando toca el suelo
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisión viene desde abajo (está aterrizando)
        if (collision.contacts[0].normal.y > 0.5f)
        {
            contadorColisionesSuelo++;
        }
    }

    // NUEVO: Detectar cuando deja de tocar el suelo
    void OnCollisionExit2D(Collision2D collision)
    {
        contadorColisionesSuelo--;
        if (contadorColisionesSuelo < 0) contadorColisionesSuelo = 0;
    }
    
    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && !estaMuerto)
        {
            vidaActual -= cantDanio;
            Debug.Log($"💔 Samurai recibió {cantDanio} de daño. Vida restante: {vidaActual}/{vidaMaxima}");

            if (healthBar != null)
            {
                healthBar.AnimarDanio();
            }

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
        puedeMover = true;
    }

    void DashLigero()
    {
        float direccion = transform.localScale.x;
        rb.linearVelocity = new Vector2(direccion * dashFuerza, rb.linearVelocity.y);
    }

    IEnumerator FinAtaque(float t, int tipoAtaque)
    {
        if (swordHitbox != null)
        {
            int danio = (tipoAtaque == 2) ? 2 : 1;
            swordHitbox.ActivarHitbox(danio);
        }

        yield return new WaitForSeconds(t);

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
        contadorColisionesSuelo = 0; // Resetear contador
        
        vidaActual = vidaMaxima;
        Debug.Log($"Samurai respawneó con {vidaActual} puntos de vida");
        
        animator.Play("Idle");
    }

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