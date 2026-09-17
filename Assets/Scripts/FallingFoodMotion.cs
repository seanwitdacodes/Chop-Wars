using UnityEngine;

/// <summary>Makes spawned food feel airborne instead of falling in perfectly rigid columns.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public sealed class FallingFoodMotion : MonoBehaviour
{
    private Rigidbody2D body;
    private float spin;
    private float swaySpeed;
    private float swayAmount;
    private float phase;

    public void Configure(float difficulty)
    {
        body = GetComponent<Rigidbody2D>();
        difficulty = Mathf.Clamp(difficulty, 1f, 3f);
        body.gravityScale *= difficulty;
        spin = Random.Range(-55f, 55f);
        swaySpeed = Random.Range(1.2f, 2.6f);
        swayAmount = Random.Range(0.18f, 0.55f);
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Awake()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
        if (body == null || body.bodyType != RigidbodyType2D.Dynamic)
        {
            return;
        }

        Vector2 velocity = body.linearVelocity;
        velocity.x = Mathf.Sin(Time.time * swaySpeed + phase) * swayAmount;
        body.linearVelocity = velocity;
        body.MoveRotation(body.rotation + spin * Time.fixedDeltaTime);
    }
}
