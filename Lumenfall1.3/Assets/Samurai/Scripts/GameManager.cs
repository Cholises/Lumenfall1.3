using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Sistema de Vida")]
    public int vidaMaximaJugador = 5;
    public int vidaActualJugador = 5;

    [Header("Habilidades Permanentes")]
    public bool habilidadDobleSalto = false;

    // 👉 NUEVO: Sistema de llaves/habilidades adicionales
    private HashSet<string> llaves = new HashSet<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetearVida()
    {
        vidaActualJugador = vidaMaximaJugador;
    }

    // -------------------------
    // 👉 SISTEMA DOUBLE JUMP
    // -------------------------
    public void ObtenerDoubleJump()
    {
        habilidadDobleSalto = true;
        Debug.Log("¡Double Jump obtenido! Puertas desbloqueadas");
    }

    public bool TieneDoubleJump()
    {
        return habilidadDobleSalto;
    }

    // -------------------------
    // 👉 SISTEMA DE LLAVES EXTRA
    // -------------------------
    public void AgregarLlave(string nombreLlave)
    {
        llaves.Add(nombreLlave);
        Debug.Log("Llave obtenida: " + nombreLlave);
    }

    public bool TieneLlave(string nombreLlave)
    {
        return llaves.Contains(nombreLlave);
    }
}
