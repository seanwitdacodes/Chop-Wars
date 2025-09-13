using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 50; // default coin value

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Find the ScoreManager in the scene
            ScoreManager manager = FindObjectOfType<ScoreManager>();

            if (manager != null)
            {
                manager.AddScore(coinValue); // 🪙 +50 points
            }
            else
            {
                Debug.LogError("⚠️ No ScoreManager found in the scene!");
            }

            Destroy(gameObject); // remove coin after pickup
        }
    }
}
