using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    // Referencia al objeto del jugador (personaje)
    public Transform target;

    // Qué tan rápido sigue la cámara al jugador
    public float smoothSpeed = 0.125f;

    // Desplazamiento opcional (por si quieres que la cámara no esté exactamente centrada)
    public Vector3 offset;

    void LateUpdate()
    {
        // Si no hay objetivo, no hacer nada
        if (target == null) return;

        // Posición deseada = posición del jugador + desplazamiento
        Vector3 desiredPosition = target.position + offset;

        // Interpolación suave entre la posición actual y la deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Actualiza la posición de la cámara (sin cambiar la profundidad del eje Z)
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
