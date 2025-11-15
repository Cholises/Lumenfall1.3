using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target; // jugador
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    [Header("Camera Limits")]
    public float minX;  
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // APLICAR LIMITES
        float clampedX = Mathf.Clamp(smoothedPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(smoothedPosition.y, minY, maxY);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
