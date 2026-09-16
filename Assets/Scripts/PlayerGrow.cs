using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGrow : MonoBehaviour
{
    [Header("Body Size")]
    public float growthAmount = 0.2f;
    public float shrinkAmount = 0.2f;
    public float minSize = 0.5f;
    public float maxSize = 3f;
    public float sizeLerpDuration = 0.12f;
    [Tooltip("Height changes much less than width so the character gets fatter or slimmer.")]
    [Range(0f, 0.5f)] public float heightGrowth = 0.12f;
    [Tooltip("Distance from the player origin to the soles, in local units.")]
    public float feetOffset = 0.88f;

    public float CurrentSize => transform.localScale.x;
    public float TargetSize => targetSize;
    public float SizeRatio => CurrentSize / Mathf.Max(0.001f, initialScale.x);

    private Coroutine scaleRoutine;
    private Vector3 initialScale;
    private float initialY;
    private float targetSize;

    private void Awake()
    {
        minSize = Mathf.Max(0.1f, minSize);
        maxSize = Mathf.Max(minSize, maxSize);
        initialScale = transform.localScale;
        initialY = transform.localPosition.y;
        targetSize = Mathf.Clamp(initialScale.x, minSize, maxSize);
        ApplySize(targetSize);
    }

    public void Grow() => AdjustSize(Mathf.Max(0f, growthAmount));
    public void Shrink() => AdjustSize(-Mathf.Max(0f, shrinkAmount));

    public void ResetSize()
    {
        targetSize = Mathf.Clamp(initialScale.x, minSize, maxSize);
        StartScaleAnimation();
    }

    private void AdjustSize(float delta)
    {
        // Accumulate from the target, so several pickups in one frame are all counted.
        targetSize = Mathf.Clamp(targetSize + delta, minSize, maxSize);
        StartScaleAnimation();
    }

    private void StartScaleAnimation()
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        if (sizeLerpDuration <= 0f || !isActiveAndEnabled)
        {
            ApplySize(targetSize);
            scaleRoutine = null;
            return;
        }
        scaleRoutine = StartCoroutine(AnimateScale());
    }

    private IEnumerator AnimateScale()
    {
        float startSize = CurrentSize;
        float elapsed = 0f;
        while (elapsed < sizeLerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / sizeLerpDuration));
            ApplySize(Mathf.Lerp(startSize, targetSize, t));
            yield return null;
        }
        ApplySize(targetSize);
        scaleRoutine = null;
    }

    private void ApplySize(float size)
    {
        float height = initialScale.y * (1f + (size / Mathf.Max(0.001f, initialScale.x) - 1f) * heightGrowth);
        transform.localScale = new Vector3(size, height, initialScale.z);
        Vector3 position = transform.localPosition;
        position.y = initialY + feetOffset * (height - initialScale.y);
        transform.localPosition = position;
    }
}
