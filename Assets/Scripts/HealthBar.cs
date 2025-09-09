using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

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
    public GameObject healthCanvas; // drag HealthCanvas here

    [Header("Options")]
    public bool autoGrabHearts = true;
    public bool drainLeftToRight = true;
    public float flashDuration = 0.2f;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    private Vector3 originalPosition;

    void Awake()
    {
        if (autoGrabHearts)
        {
            hearts = GetComponentsInChildren<Image>(true);
            hearts = hearts.OrderBy(h => h.transform.position.x).ToArray();
        }

        originalPosition = transform.localPosition;
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        if (healthCanvas != null)
            healthCanvas.SetActive(true);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHearts();

        if (currentHealth > 0)
        {
            int emptyCount = maxHealth - currentHealth;
            int index = drainLeftToRight ? emptyCount - 1 : (hearts.Length - emptyCount);

            if (index >= 0 && index < hearts.Length)
                StartCoroutine(FlashHeart(hearts[index], Color.red));

            StartCoroutine(ShakeBar());
        }
        else
        {
            HideHealthBar();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHearts();

        int index = drainLeftToRight ? (currentHealth - 1) : (hearts.Length - currentHealth);
        if (index >= 0 && index < hearts.Length)
            StartCoroutine(FlashHeart(hearts[index], Color.green));
    }

    public void UpdateHearts()
    {
        int emptyCount = maxHealth - currentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            int index = drainLeftToRight ? i : (hearts.Length - 1 - i);

            if (i < emptyCount)
                hearts[index].sprite = emptyHeart;
            else
                hearts[index].sprite = fullHeart;
        }
    }

    private IEnumerator FlashHeart(Image heart, Color flashColor)
    {
        Color originalColor = heart.color;
        heart.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        heart.color = originalColor;
    }

    private IEnumerator ShakeBar()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPosition;
    }

    public void HideHealthBar()
    {
        if (healthCanvas != null)
            healthCanvas.SetActive(false);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHearts();

        if (healthCanvas != null)
            healthCanvas.SetActive(true);
    }
}
