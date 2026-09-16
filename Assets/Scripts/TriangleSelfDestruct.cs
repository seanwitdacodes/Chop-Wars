using UnityEngine;

public class TriangleSelfDestruct : MonoBehaviour
{
    public float destroyY = -5f;
    public float maximumLifetime = 30f;

    private Camera gameplayCamera;
    private SpriteRenderer itemSprite;
    private float lifetime;

    private void Awake()
    {
        gameplayCamera = Camera.main;
        itemSprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        lifetime += Time.deltaTime;
        float bottom = destroyY;
        if (gameplayCamera != null && gameplayCamera.orthographic)
        {
            bottom = Mathf.Min(bottom, gameplayCamera.transform.position.y - gameplayCamera.orthographicSize);
        }

        float highestPoint = itemSprite != null ? itemSprite.bounds.max.y : transform.position.y;
        if (highestPoint < bottom || (maximumLifetime > 0f && lifetime >= maximumLifetime))
        {
            Destroy(gameObject);
        }
    }
}
