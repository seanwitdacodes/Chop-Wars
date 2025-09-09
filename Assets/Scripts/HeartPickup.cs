using UnityEngine;

public class HeartPickup : MonoBehaviour
{
    public int healAmount = 1; // how many hearts this pickup restores

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Heart")) // Player must be tagged "Player"
        {
            HealthBar healthBar = FindObjectOfType<HealthBar>();
            if (healthBar != null && healthBar.currentHealth < healthBar.maxHealth)
            {
                healthBar.Heal(healAmount);
            }

            Destroy(gameObject); // destroy the heart on impact
        }
    }
}
