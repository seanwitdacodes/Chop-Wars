using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float minX = -8.5f;
    public float maxX = 8.5f;

    [Header("Speed Adjustments")]
    public float minSpeed = 0f;
    public float maxSpeed = 13f;
    public float speedChange = 0.5f;

    [Header("Game Over Settings")]
    public int maxHits = 10;

    private Rigidbody2D rb;
    private ScoreManager scoreManager;

    [Header("UI References")]
    public HealthBar healthBar; // drag HealthBar from canvas in Inspector

    private bool isGameOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        moveSpeed = 5f;

        scoreManager = FindObjectOfType<ScoreManager>();

        if (healthBar != null)
        {
            healthBar.maxHealth = maxHits;
            healthBar.currentHealth = maxHits;
            healthBar.UpdateHearts();
        }
    }

    void Update()
    {
        if (isGameOver) return;

        float moveX = Input.GetAxisRaw("Horizontal");

        if (moveSpeed > 0f)
        {
            float newX = transform.position.x + moveX * moveSpeed * Time.deltaTime;
            newX = Mathf.Clamp(newX, minX, maxX);
            transform.position = new Vector2(newX, transform.position.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return;

        //  Enemy hit
        if (other.CompareTag("Enemy"))
        {
            moveSpeed = Mathf.Max(minSpeed, moveSpeed - speedChange);

            if (healthBar != null)
            {
                healthBar.TakeDamage(1);
                Debug.Log("Enemy hit! Health = " + healthBar.currentHealth);
            }

            if (healthBar.currentHealth <= 0)
            {
                GameOver();
            }

            Destroy(other.gameObject); // remove enemy on impact
        }

        //  Healthy food
        else if (other.CompareTag("Healthy"))
        {
            if (moveSpeed < maxSpeed)
            {
                moveSpeed = Mathf.Min(maxSpeed, moveSpeed + speedChange);
                Debug.Log("Healthy fruit collected! Speed increased to: " + moveSpeed);
            }
            else
            {
                Debug.Log("Healthy fruit collected! Already at max speed.");
            }

            Destroy(other.gameObject); // remove healthy pickup
        }

        //  Heart pickup
        else if (other.CompareTag("Heart"))
        {
            if (healthBar != null)
            {
                if (healthBar.currentHealth < healthBar.maxHealth)
                {
                    healthBar.currentHealth++;
                    healthBar.UpdateHearts();
                    Debug.Log("❤️ Heart collected! Health = " + healthBar.currentHealth);
                }
                else
                {
                    Debug.Log("❤️ Heart collected but already at max health!");
                }
            }

            Destroy(other.gameObject); // remove heart pickup
        }

        //  🪙 Coin pickup
        else if (other.CompareTag("Coin"))
        {
            if (scoreManager != null)
            {
                scoreManager.AddScore(50); // ✅ add 50 points
                Debug.Log("🪙 Coin collected! +50 points, total score = " + scoreManager.GetScore());
            }

            Destroy(other.gameObject); // remove coin pickup
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("GAME OVER!");

        if (scoreManager != null)
        {
            scoreManager.HideScoreboard();
        }

        // Show lose screen
        LoseScreenManager loseManager = FindObjectOfType<LoseScreenManager>();
        if (loseManager != null)
        {
            loseManager.ShowLoseScreen();
        }

        // Disable spawners
        Spawner[] spawners = FindObjectsOfType<Spawner>();
        foreach (var spawner in spawners)
        {
            spawner.enabled = false;
        }
    }
}
