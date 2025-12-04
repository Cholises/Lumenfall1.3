using UnityEngine;

public class PotionAnimation : MonoBehaviour
{
    [Header("Floating")]
    public float floatSpeed = 2f;
    public float floatAmount = 0.15f;

    [Header("Glow")]
    public float glowSpeed = 3f;
    public float glowIntensity = 0.2f;

    private Vector3 startPos;
    private SpriteRenderer sr;

    void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Flotación
        float y = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = startPos + new Vector3(0, y, 0);

        // Parpadeo suave (brillo)
        float glow = (Mathf.Sin(Time.time * glowSpeed) + 1f) * 0.5f;
        float alpha = 1 - (glow * glowIntensity);
        sr.color = new Color(1f, 1f, 1f, alpha);
    }
}
