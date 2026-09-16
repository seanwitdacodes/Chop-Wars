using UnityEngine;
using UnityEngine.EventSystems;

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

    [Header("Pointer Controls")]
    public bool allowPointerInput = true;
    public float pointerMoveSpeed = 18f;

    [Header("Game Over Settings")]
    public int maxHits = 10;

    [Header("Feedback")]
    public float hitStopDuration = 0.05f;
    public float flashDuration = 0.12f;
    public Color damageFlashColor = new Color(1f, 0.55f, 0.55f, 1f);
    public Color pickupFlashColor = new Color(0.8f, 1f, 0.8f, 1f);
    public Color healFlashColor = new Color(1f, 0.92f, 0.6f, 1f);
    public Color winFlashColor = new Color(1f, 0.92f, 0.45f, 1f);
    public float recoveryDuration = 0.8f;

    [Header("UI References")]
    public HealthBar healthBar;

    private Rigidbody2D rb;
    private ScoreManager scoreManager;
    private PlayerGrow playerGrow;
    private SpriteRenderer playerSprite;
    private PlayerCharacterVisual characterVisual;
    private Collider2D playerCollider;
    private Camera gameplayCamera;
    private PauseManager pauseManager;
    private Coroutine flashRoutine;
    private Coroutine hitStopRoutine;
    private Color originalSpriteColor = Color.white;
    private int currentHits;
    private bool isRoundOver = false;
    private bool isRecovering;
    private float horizontalInput;
    private float pointerTargetX;
    private bool hasPointerTarget;
    private float startingMoveSpeed;

    public bool IsRoundOver => isRoundOver;
    public bool CanCollectPickups => !isRoundOver && !isRecovering && (pauseManager == null || !pauseManager.IsPaused);
    public int CurrentHealth => currentHits;
    public bool IsRecovering => isRecovering;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        maxHits = Mathf.Max(1, maxHits);
        minSpeed = Mathf.Max(0f, minSpeed);
        maxSpeed = Mathf.Max(minSpeed, maxSpeed);
        moveSpeed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
        startingMoveSpeed = Mathf.Max(0.01f, moveSpeed);
        speedChange = Mathf.Max(0f, speedChange);

        scoreManager = FindAnyObjectByType<ScoreManager>();
        playerGrow = GetComponent<PlayerGrow>();
        playerSprite = GetComponent<SpriteRenderer>();
        characterVisual = GetComponent<PlayerCharacterVisual>();
        playerCollider = GetComponent<Collider2D>();
        gameplayCamera = Camera.main;
        pauseManager = FindAnyObjectByType<PauseManager>();
        currentHits = maxHits;

        if (playerSprite != null)
        {
            originalSpriteColor = playerSprite.color;
        }

        if (healthBar == null)
        {
            healthBar = FindAnyObjectByType<HealthBar>();
        }

        if (healthBar != null)
        {
            healthBar.maxHealth = maxHits;
            healthBar.ResetHealth();
            currentHits = healthBar.currentHealth;
        }
        else
        {
            Debug.LogWarning("PlayerMovement: No HealthBar assigned or found in the scene.");
        }

    }

    private void Update()
    {
        if (isRoundOver || Time.timeScale == 0f)
        {
            horizontalInput = 0f;
            hasPointerTarget = false;
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        hasPointerTarget = allowPointerInput && TryGetPointerTargetX(out pointerTargetX);
    }

    private void FixedUpdate()
    {
        if (isRoundOver || rb == null)
        {
            return;
        }

        float newX;
        if (hasPointerTarget)
        {
            float currentPointerSpeed = Mathf.Max(0f, pointerMoveSpeed) * moveSpeed / startingMoveSpeed;
            newX = Mathf.MoveTowards(rb.position.x, pointerTargetX, currentPointerSpeed * Time.fixedDeltaTime);
        }
        else
        {
            newX = rb.position.x + horizontalInput * moveSpeed * Time.fixedDeltaTime;
        }

        GetMovementBounds(out float left, out float right);
        rb.MovePosition(new Vector2(Mathf.Clamp(newX, left, right), transform.position.y));
    }

    private void GetMovementBounds(out float left, out float right)
    {
        left = Mathf.Min(minX, maxX);
        right = Mathf.Max(minX, maxX);

        if (gameplayCamera != null && gameplayCamera.orthographic)
        {
            float cameraHalfWidth = gameplayCamera.orthographicSize * gameplayCamera.aspect;
            left = Mathf.Max(left, gameplayCamera.transform.position.x - cameraHalfWidth);
            right = Mathf.Min(right, gameplayCamera.transform.position.x + cameraHalfWidth);
        }

        float halfWidth = playerCollider != null ? playerCollider.bounds.extents.x : 0f;
        if (characterVisual != null)
        {
            halfWidth = Mathf.Max(halfWidth, characterVisual.HalfWidth);
        }

        float center = (left + right) * 0.5f;
        left = Mathf.Min(left + halfWidth, center);
        right = Mathf.Max(right - halfWidth, center);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanCollectPickups || !other.enabled || !other.gameObject.activeInHierarchy)
        {
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            ConsumePickup(other);
            moveSpeed = Mathf.Max(minSpeed, moveSpeed - speedChange);
            currentHits = Mathf.Max(0, currentHits - 1);
            playerGrow?.Grow();
            scoreManager?.BreakCombo();

            if (healthBar != null)
            {
                healthBar.TakeDamage(1);
            }

            PlayFlash(damageFlashColor);
            StartHitStop();

            if (currentHits <= 0)
            {
                StartCoroutine(RecoverFromEmptyHealth());
            }
        }
        else if (other.CompareTag("Healthy"))
        {
            ConsumePickup(other);
            if (moveSpeed < maxSpeed)
            {
                moveSpeed = Mathf.Min(maxSpeed, moveSpeed + speedChange);
            }

            playerGrow?.Shrink();
            scoreManager?.RegisterHealthyPickup();
            if (currentHits < maxHits)
            {
                currentHits++;
                healthBar?.Heal(1);
            }

            PlayFlash(pickupFlashColor);
        }
        else if (other.CompareTag("Heart"))
        {
            ConsumePickup(other);
            if (currentHits < maxHits)
            {
                currentHits++;

                if (healthBar != null)
                {
                    healthBar.Heal(1);
                }

                PlayFlash(healFlashColor);
            }
        }
    }

    private static void ConsumePickup(Collider2D pickupCollider)
    {
        GameObject pickup = pickupCollider.attachedRigidbody != null
            ? pickupCollider.attachedRigidbody.gameObject
            : pickupCollider.gameObject;
        // Disable immediately so multiple collider callbacks cannot count one pickup twice.
        pickup.SetActive(false);
        Destroy(pickup);
    }

    private System.Collections.IEnumerator RecoverFromEmptyHealth()
    {
        if (isRecovering)
        {
            yield break;
        }

        isRecovering = true;
        horizontalInput = 0f;
        hasPointerTarget = false;
        playerGrow?.ResetSize();
        moveSpeed = startingMoveSpeed;
        PlayFlash(winFlashColor);
        yield return new WaitForSeconds(Mathf.Max(0.1f, recoveryDuration));

        currentHits = maxHits;
        healthBar?.ResetHealth();
        if (rb != null)
        {
            rb.position = new Vector2(0f, rb.position.y);
        }

        isRecovering = false;
    }

    private bool TryGetPointerTargetX(out float targetX)
    {
        targetX = transform.position.x;

        Vector3 screenPosition;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled ||
                (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)))
            {
                return false;
            }

            screenPosition = touch.position;
        }
        else if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            screenPosition = Input.mousePosition;
        }
        else
        {
            return false;
        }

        Camera activeCamera = gameplayCamera;
        if (activeCamera == null)
        {
            return false;
        }

        screenPosition.z = Mathf.Abs(transform.position.z - activeCamera.transform.position.z);
        Vector3 worldPoint = activeCamera.ScreenToWorldPoint(screenPosition);
        GetMovementBounds(out float left, out float right);
        targetX = Mathf.Clamp(worldPoint.x, left, right);
        return true;
    }

    private void PlayFlash(Color flashColor)
    {
        if (playerSprite == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashSpriteRoutine(flashColor));
    }

    private System.Collections.IEnumerator FlashSpriteRoutine(Color flashColor)
    {
        playerSprite.color = flashColor;
        yield return new WaitForSecondsRealtime(flashDuration);
        playerSprite.color = originalSpriteColor;
        flashRoutine = null;
    }

    private void StartHitStop()
    {
        if (hitStopDuration <= 0f || hitStopRoutine != null || currentHits <= 0)
        {
            return;
        }

        hitStopRoutine = StartCoroutine(HitStopRoutine());
    }

    private System.Collections.IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopDuration);

        if (!isRoundOver && (pauseManager == null || !pauseManager.IsPaused))
        {
            Time.timeScale = 1f;
        }

        hitStopRoutine = null;
    }

}
