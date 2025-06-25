using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public Transform target;  // player to follow
    public Vector3 offset = new Vector3(0, 1f, 0); // above the player

    public void SetHealth(float health)
    {
        health = Mathf.Clamp(health, 0f, 40f);
        fillImage.fillAmount = health / 40f;
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
            screenPos.y += 30f; 
            transform.position = screenPos;
        }
    }
}
