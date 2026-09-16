using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Heart Settings (auto-filled)")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("References")]
    public GameObject healthCanvas;

    [Header("Options")]
    public bool autoGrabHearts = true;
    public bool drainLeftToRight = true;
    public float flashDuration = 0.2f;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    private Vector3 originalPosition;
    private Color[] originalColors;
    private Coroutine[] heartFlashes;
    private Coroutine shakeRoutine;
    private bool initialized;

    private void Awake()
    {
        Initialize();
        ResetHealth();
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        maxHealth = Mathf.Max(1, maxHealth);
        originalPosition = transform.localPosition;

        if (autoGrabHearts)
        {
            // Exclude a bar background or other decorations from the heart slots.
            hearts = GetComponentsInChildren<Image>(true)
                .Where(heart => heart.transform != transform &&
                    ((fullHeart == null && emptyHeart == null) ||
                     heart.sprite == fullHeart || heart.sprite == emptyHeart))
                .OrderBy(heart => heart.transform.position.x)
                .ThenBy(heart => heart.transform.GetSiblingIndex())
                .ToArray();
        }

        hearts = hearts ?? Array.Empty<Image>();
        originalColors = new Color[hearts.Length];
        heartFlashes = new Coroutine[hearts.Length];
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                originalColors[i] = hearts[i].color;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        int oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHearts();

        if (currentHealth == 0)
        {
            HideHealthBar();
            return;
        }

        FlashChangedHearts(oldHealth, Color.red);
        if (isActiveAndEnabled)
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeBar());
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int oldHealth = currentHealth;
        currentHealth = (int)Math.Min(maxHealth, (long)currentHealth + amount);
        UpdateHearts();
        FlashChangedHearts(oldHealth, Color.green);
    }

    public void UpdateHearts()
    {
        Initialize();
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        int emptyCount = maxHealth - currentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            int index = HeartIndex(i);
            if (hearts[index] == null)
            {
                continue;
            }

            hearts[index].enabled = i < maxHealth;
            hearts[index].sprite = i < emptyCount ? emptyHeart : fullHeart;
        }
    }

    private int HeartIndex(int drainIndex)
    {
        return drainLeftToRight ? drainIndex : hearts.Length - 1 - drainIndex;
    }

    private void FlashChangedHearts(int oldHealth, Color color)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        int firstChanged = maxHealth - Mathf.Max(oldHealth, currentHealth);
        int lastChanged = maxHealth - Mathf.Min(oldHealth, currentHealth);
        for (int i = firstChanged; i < lastChanged && i < hearts.Length; i++)
        {
            int index = HeartIndex(i);
            if (index < 0 || index >= hearts.Length || hearts[index] == null)
            {
                continue;
            }

            if (heartFlashes[index] != null)
            {
                StopCoroutine(heartFlashes[index]);
            }

            heartFlashes[index] = StartCoroutine(FlashHeart(index, color));
        }
    }

    private IEnumerator FlashHeart(int index, Color flashColor)
    {
        hearts[index].color = flashColor;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, flashDuration));
        if (hearts[index] != null)
        {
            hearts[index].color = originalColors[index];
        }

        heartFlashes[index] = null;
    }

    private IEnumerator ShakeBar()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            transform.localPosition = originalPosition + new Vector3(x, y, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }

    public void HideHealthBar()
    {
        if (healthCanvas != null)
        {
            healthCanvas.SetActive(false);
        }
    }

    public void ResetHealth()
    {
        Initialize();
        ResetFeedback();
        currentHealth = Mathf.Max(1, maxHealth);
        UpdateHearts();
        if (healthCanvas != null)
        {
            healthCanvas.SetActive(true);
        }
    }

    private void OnDisable()
    {
        ResetFeedback();
    }

    private void ResetFeedback()
    {
        if (!initialized)
        {
            return;
        }

        StopAllCoroutines();
        shakeRoutine = null;
        transform.localPosition = originalPosition;
        for (int i = 0; i < hearts.Length; i++)
        {
            heartFlashes[i] = null;
            if (hearts[i] != null)
            {
                hearts[i].color = originalColors[i];
            }
        }
    }
}
