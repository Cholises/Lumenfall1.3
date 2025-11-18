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
    public float dashFuerza = 4f;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb != null)
            originalGravityScale = rb.gravityScale;

        posicionInicial = transform.position;
    }

    void Start()
    {
        // Código adicional si lo necesitas
    }

    void Update()
    {
        if (estaMuerto) return;

        Vector2 origenRaycast = new Vector2(transform.position.x, transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origenRaycast, Vector2.down, longitudRaycast, capaSuelo);
        enSuelo = hit.collider != null;
        animator.SetBool("ensuelo", enSuelo);
        animator.SetBool("recibeDanio", recibiendoDanio);

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
                StartCoroutine(FinAtaque(0.5f));
            }

            if (Input.GetKeyDown(KeyCode.K) && !atacando && !atacando2)
            {
                atacando2 = true;
                animator.SetTrigger("Ataque2");
                DashLigero();
                StartCoroutine(FinAtaque(0.7f));
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
            StopCoroutine("HurtRutina"); // Detener cualquier rutina anterior
            StartCoroutine(HurtRutina());
            Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized;
            rb.AddForce(rebote * 5f, ForceMode2D.Impulse);
        }
    }

    public void DesactivaDanio()
    {
        StopCoroutine("HurtRutina");
        recibiendoDanio = false;
        puedeMover = true;
        animator.SetBool("recibeDanio", false);
    }

    void DashLigero()
    {
        float direccion = transform.localScale.x;
        rb.linearVelocity = new Vector2(direccion * dashFuerza, rb.linearVelocity.y);
    }

    IEnumerator FinAtaque(float t)
    {
        yield return new WaitForSeconds(t);
        atacando = false;
        atacando2 = false;
    }

    IEnumerator HurtRutina()
    {
        if (estaMuerto) yield break;

        recibiendoDanio = true;
        puedeMover = false;
        animator.SetTrigger("Hurt");

        yield return new WaitForSeconds(0.3f);
        
        recibiendoDanio = false;
        puedeMover = true;
    }

    public void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        puedeMover = false;

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
        animator.Play("Idle");
    }
}