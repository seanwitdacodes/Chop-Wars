using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A layered, animated chef made from the player's existing circle sprite.
/// The torso follows PlayerGrow; the face and limbs keep recognizable proportions.
/// Generated children are rebuilt in the editor and are never saved into the scene.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(PlayerGrow))]
public class PlayerCharacterVisual : MonoBehaviour
{
    [Header("Character Palette")]
    public Color skinColor = new Color(0.60f, 0.32f, 0.18f);
    public Color apronColor = new Color(0.04f, 0.65f, 0.57f);
    public Color scarfColor = new Color(1f, 0.30f, 0.21f);
    public Color outlineColor = new Color(0.10f, 0.15f, 0.20f);

    public float HalfWidth => Mathf.Max(0.54f * Mathf.Abs(transform.lossyScale.x),
        0.53f * Mathf.Abs(transform.lossyScale.x) + 0.23f);

    private readonly List<SpriteRenderer> parts = new List<SpriteRenderer>();
    private readonly List<Color> colors = new List<Color>();
    private SpriteRenderer source;
    private Transform visual;
    private Transform head;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftFoot;
    private Transform rightFoot;
    private Transform leftEye;
    private Transform rightEye;
    private Vector3 previousPosition;
    private float walkCycle;
    private float lean;
    private bool sourceWasEnabled;

    private void OnEnable()
    {
        source = GetComponent<SpriteRenderer>();
        sourceWasEnabled = source.enabled;
        if (source.sprite == null) return;
        BuildCharacter();
        source.enabled = false;
        previousPosition = transform.position;
        Pose(0f);
    }

    private Transform Group(string label, Transform parent, Vector2 position)
    {
        var item = new GameObject(label);
        item.hideFlags = HideFlags.HideAndDontSave;
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        return item.transform;
    }

    private SpriteRenderer Oval(string label, Transform parent, Vector2 position, Vector2 size,
        Color color, int order, float angle = 0f)
    {
        Transform item = Group(label, parent, position);
        Vector2 spriteSize = source.sprite.bounds.size;
        item.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);
        item.localRotation = Quaternion.Euler(0f, 0f, angle);
        SpriteRenderer renderer = item.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = source.sprite;
        renderer.sharedMaterial = source.sharedMaterial;
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = source.sortingOrder + order;
        renderer.color = color;
        parts.Add(renderer);
        colors.Add(color);
        return renderer;
    }

    private void BuildCharacter()
    {
        visual = Group("Character Visual", transform, Vector2.zero);
        Color cream = new Color(1f, 0.97f, 0.86f);
        Color white = new Color(1f, 0.99f, 0.95f);
        Color skinLight = Color.Lerp(skinColor, cream, 0.20f);

        leftFoot = Group("Left Foot", visual, new Vector2(-0.24f, -0.75f));
        rightFoot = Group("Right Foot", visual, new Vector2(0.24f, -0.75f));
        foreach (Transform foot in new[] { leftFoot, rightFoot })
        {
            Oval("Trouser", foot, new Vector2(0f, 0.11f), new Vector2(0.23f, 0.35f), outlineColor, 2);
            Oval("Shoe", foot, Vector2.zero, new Vector2(0.34f, 0.26f), outlineColor, 3);
            Oval("Sole", foot, new Vector2(0f, -0.07f), new Vector2(0.30f, 0.065f), cream, 4);
        }

        leftArm = Group("Left Arm", visual, new Vector2(-0.53f, -0.10f));
        rightArm = Group("Right Arm", visual, new Vector2(0.53f, -0.10f));
        foreach (Transform arm in new[] { leftArm, rightArm })
        {
            Oval("Sleeve Outline", arm, Vector2.zero, new Vector2(0.30f, 0.48f), outlineColor, 4);
            Oval("Sleeve", arm, new Vector2(0f, 0.025f), new Vector2(0.24f, 0.38f), cream, 5);
            Oval("Hand Outline", arm, new Vector2(0f, -0.24f), new Vector2(0.28f, 0.29f), outlineColor, 5);
            Oval("Hand", arm, new Vector2(0f, -0.23f), new Vector2(0.22f, 0.23f), skinLight, 6);
        }

        Oval("Body Outline", visual, new Vector2(0f, -0.16f), new Vector2(1.08f, 1.10f), outlineColor, 7);
        Oval("Body", visual, new Vector2(0f, -0.14f), new Vector2(1f, 1.02f), cream, 8);
        Oval("Apron", visual, new Vector2(0f, -0.23f), new Vector2(0.86f, 0.83f), apronColor, 9);
        Oval("Apron Highlight", visual, new Vector2(-0.20f, -0.17f), new Vector2(0.12f, 0.48f), Color.Lerp(apronColor, white, 0.17f), 10, -10f);
        Oval("Pocket Outline", visual, new Vector2(0f, -0.33f), new Vector2(0.35f, 0.23f), outlineColor, 10);
        Oval("Pocket", visual, new Vector2(0f, -0.30f), new Vector2(0.30f, 0.18f), Color.Lerp(apronColor, white, 0.3f), 11);
        Oval("Scarf Left", visual, new Vector2(-0.14f, 0.25f), new Vector2(0.37f, 0.15f), scarfColor, 12, -25f);
        Oval("Scarf Right", visual, new Vector2(0.14f, 0.25f), new Vector2(0.37f, 0.15f), scarfColor, 12, 25f);
        Oval("Scarf Knot", visual, new Vector2(0f, 0.19f), new Vector2(0.17f, 0.18f), scarfColor, 13);

        head = Group("Head", visual, new Vector2(0f, 0.51f));
        Oval("Left Ear", head, new Vector2(-0.34f, -0.01f), new Vector2(0.19f, 0.23f), outlineColor, 13);
        Oval("Right Ear", head, new Vector2(0.34f, -0.01f), new Vector2(0.19f, 0.23f), outlineColor, 13);
        Oval("Face Outline", head, Vector2.zero, new Vector2(0.77f, 0.74f), outlineColor, 14);
        Oval("Face", head, new Vector2(0f, 0.015f), new Vector2(0.68f, 0.64f), skinColor, 15);
        Oval("Cheek Left", head, new Vector2(-0.23f, -0.09f), new Vector2(0.15f, 0.08f), skinLight, 16);
        Oval("Cheek Right", head, new Vector2(0.23f, -0.09f), new Vector2(0.15f, 0.08f), skinLight, 16);
        leftEye = Group("Left Eye", head, new Vector2(-0.15f, 0.045f));
        rightEye = Group("Right Eye", head, new Vector2(0.15f, 0.045f));
        foreach (Transform eye in new[] { leftEye, rightEye })
        {
            Oval("Eye White", eye, Vector2.zero, new Vector2(0.15f, 0.20f), white, 17);
            Oval("Pupil", eye, new Vector2(0.015f, 0f), new Vector2(0.075f, 0.115f), outlineColor, 18);
            Oval("Eye Spark", eye, new Vector2(0.028f, 0.028f), new Vector2(0.027f, 0.038f), white, 19);
        }
        Oval("Nose", head, new Vector2(0f, -0.04f), new Vector2(0.12f, 0.13f), skinLight, 20);
        Oval("Smile", head, new Vector2(0f, -0.16f), new Vector2(0.28f, 0.14f), outlineColor, 17);
        Oval("Teeth", head, new Vector2(0f, -0.13f), new Vector2(0.21f, 0.055f), white, 18);

        Oval("Hat Outline", head, new Vector2(0f, 0.38f), new Vector2(0.94f, 0.23f), outlineColor, 21);
        Oval("Hat Band", head, new Vector2(0f, 0.39f), new Vector2(0.84f, 0.16f), cream, 22);
        Oval("Hat Left Outline", head, new Vector2(-0.25f, 0.55f), new Vector2(0.44f, 0.40f), outlineColor, 20);
        Oval("Hat Right Outline", head, new Vector2(0.25f, 0.55f), new Vector2(0.44f, 0.40f), outlineColor, 20);
        Oval("Hat Top Outline", head, new Vector2(0f, 0.64f), new Vector2(0.49f, 0.44f), outlineColor, 20);
        Oval("Hat Left", head, new Vector2(-0.25f, 0.55f), new Vector2(0.35f, 0.31f), white, 21);
        Oval("Hat Right", head, new Vector2(0.25f, 0.55f), new Vector2(0.35f, 0.31f), white, 21);
        Oval("Hat Top", head, new Vector2(0f, 0.64f), new Vector2(0.40f, 0.35f), white, 21);
    }

    private void LateUpdate()
    {
        if (visual == null) return;
        float velocity = Application.isPlaying && Time.deltaTime > 0f
            ? (transform.position.x - previousPosition.x) / Time.deltaTime : 0f;
        previousPosition = transform.position;
        Pose(velocity);
        for (int i = 0; i < parts.Count; i++)
            if (parts[i] != null) parts[i].color = colors[i] * source.color;
    }

    private void Pose(float velocity)
    {
        float width = Mathf.Max(0.1f, Mathf.Abs(transform.localScale.x));
        float height = Mathf.Max(0.1f, Mathf.Abs(transform.localScale.y));
        float step = Application.isPlaying ? Time.deltaTime : 0f;
        lean = Mathf.Lerp(lean, Mathf.Clamp(velocity * 0.7f, -7f, 7f), 12f * step);
        walkCycle += Mathf.Abs(velocity) * step * 3.5f;
        float stride = Mathf.Sin(walkCycle) * Mathf.Clamp01(Mathf.Abs(velocity) / 2f);
        head.localScale = new Vector3(Mathf.Pow(width, 0.12f) / width, 1f / height, 1f);
        head.localRotation = Quaternion.Euler(0f, 0f, -lean * 0.5f);
        leftArm.localScale = rightArm.localScale = new Vector3(1f / width, 1f / height, 1f);
        leftArm.localRotation = Quaternion.Euler(0f, 0f, -12f + stride * 15f);
        rightArm.localRotation = Quaternion.Euler(0f, 0f, 12f - stride * 15f);
        float footX = 0.24f / Mathf.Sqrt(width);
        float footY = -0.88f + 0.13f / height;
        leftFoot.localPosition = new Vector3(-footX, footY + Mathf.Max(0f, stride) * 0.045f, 0f);
        rightFoot.localPosition = new Vector3(footX, footY + Mathf.Max(0f, -stride) * 0.045f, 0f);
        leftFoot.localScale = rightFoot.localScale = new Vector3(1f / width, 1f / height, 1f);
        float blink = Application.isPlaying && Time.time % 4.3f > 4.15f ? 0.12f : 1f;
        leftEye.localScale = rightEye.localScale = new Vector3(1f, blink, 1f);
    }

    private void OnDisable()
    {
        if (source != null) source.enabled = sourceWasEnabled;
        if (visual != null)
        {
            if (Application.isPlaying) Destroy(visual.gameObject);
            else DestroyImmediate(visual.gameObject);
        }
        parts.Clear();
        colors.Clear();
    }
}
