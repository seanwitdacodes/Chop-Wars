using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerGrow : MonoBehaviour
{
    [Header("Size Settings")]
    public float growthAmount = 0.2f;    // Amount player grows
    public float shrinkAmount = 0.2f;    // Amount player shrinks
    public float minSize = 0.5f;         // Minimum scale
    public float maxSize = 3f;           // Maximum scale

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    // Collision with solid objects
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    // Collision with trigger objects
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void HandleCollision(GameObject obj)
    {
        if (obj.CompareTag("Enemy"))
        {
            Grow();
            Destroy(obj);
        }
        else if (obj.CompareTag("Healthy"))
        {
            Shrink();
            Destroy(obj);
        }
    }

    private void Grow()
    {
        Vector3 newScale = transform.localScale + new Vector3(growthAmount, growthAmount, 0f);
        transform.localScale = Vector3.Min(newScale, new Vector3(maxSize, maxSize, 1f));
    }

    private void Shrink()
    {
        Vector3 newScale = transform.localScale - new Vector3(shrinkAmount, shrinkAmount, 0f);
        transform.localScale = Vector3.Max(newScale, new Vector3(minSize, minSize, 1f));
    }
}
