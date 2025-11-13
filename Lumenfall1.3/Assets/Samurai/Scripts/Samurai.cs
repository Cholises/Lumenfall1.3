using UnityEngine;
using System.Collections;

public class Samurai : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;
    public float longitudRaycast = 0.1f; // 🔹 Más corto y preciso
    public LayerMask capaSuelo;

    [Header("Ataque")]
    public float dashFuerza = 4f; // 🔹 Fuerza del dash al atacar

    [Header("Referencias")]
    public Animator animator;

    private Rigidbody2D rb;
    private bool enSuelo;
    private bool atacando;
    private bool atacando2;
    private bool estaMuerto;
    private bool puedeMover = true;

    private Vector3 posicionInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponent<Animator>();

        posicionInicial = transform.position; // 🔹 Guarda la posición inicial del personaje
    }

    void Update()
    {
        if (estaMuerto) return;

        // Detectar suelo con raycast desde los pies
        Vector2 origenRaycast = new Vector2(transform.position.x, transform.position.y - 0.5f);
        RaycastHit2D hit = Physics2D.Raycast(origenRaycast, Vector2.down, longitudRaycast, capaSuelo);
        enSuelo = hit.collider != null;
        animator.SetBool("ensuelo", enSuelo);

        // Movimiento horizontal
        float inputX = Input.GetAxisRaw("Horizontal");
        float movimiento = inputX * velocidad * Time.deltaTime;
        animator.SetFloat("Movement", Mathf.Abs(inputX));

        if (inputX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (inputX > 0)
            transform.localScale = new Vector3(1, 1, 1);

        if (puedeMover)
        {
            transform.position += new Vector3(movimiento, 0, 0);
        }

        // Salto (solo cuando está en el suelo)
        if (Input.GetKeyDown(KeyCode.Space) && enSuelo && puedeMover)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        }

        // ATAQUE 1 (J) con dash
        if (Input.GetKeyDown(KeyCode.J) && !atacando && !atacando2 && puedeMover)
        {
            atacando = true;
            animator.SetTrigger("Ataque");
            StartCoroutine(FinAtaque(0.5f));
        }

        // ATAQUE 2 (K) con dash
        if (Input.GetKeyDown(KeyCode.K) && !atacando && !atacando2 && puedeMover)
        {
            atacando2 = true;
            animator.SetTrigger("Ataque2");
            DashLigero();
            StartCoroutine(FinAtaque(0.7f));
        }

        // HURT (H)
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(HurtRutina());
        }

        // DEATH (L)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Morir();
        }
    }

    void DashLigero()
    {
        float direccion = transform.localScale.x;
        rb.linearVelocity = new Vector2(direccion * dashFuerza, rb.linearVelocity.y);
    }

    IEnumerator FinAtaque(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        atacando = false;
        atacando2 = false;
    }

    IEnumerator HurtRutina()
    {
        if (estaMuerto) yield break;

        puedeMover = false;
        animator.SetTrigger("Hurt");

        yield return new WaitForSeconds(0.5f);
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

        StartCoroutine(RespawnDespues(2f)); // Espera a que termine la animación
    }

    IEnumerator RespawnDespues(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        transform.position = posicionInicial; // 🔹 Reaparece donde lo colocaste
        estaMuerto = false;
        puedeMover = true;
        animator.Play("Idle");
    }

    void OnDrawGizmos()
    {
        // 🔹 Visualiza el raycast desde los pies
        Vector2 origenRaycast = new Vector2(transform.position.x, transform.position.y - 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origenRaycast, origenRaycast + Vector2.down * longitudRaycast);
    }
}