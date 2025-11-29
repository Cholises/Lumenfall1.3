using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Sistema de Vida")]
    public int vidaMaximaJugador = 5;
    public int vidaActualJugador = 5;

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
}