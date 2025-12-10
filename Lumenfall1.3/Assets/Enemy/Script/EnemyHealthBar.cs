using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fillImage;
    public NightBorne enemy;

    void Update()
    {
        if (enemy == null) return;

        float t = (float)enemy.GetCurrentHealth() / enemy.maxHealth;
        fillImage.fillAmount = t;

        // Siempre mirar a la cámara (opcional)
        transform.rotation = Camera.main.transform.rotation;
    }
}