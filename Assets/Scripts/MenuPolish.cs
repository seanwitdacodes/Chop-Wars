using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gives the sprite-based menus larger, responsive button cards while preserving
/// their original art and serialized click actions.
/// </summary>
public sealed class MenuPolish : MonoBehaviour
{
    private static Sprite roundedSprite;
    private readonly List<MenuButtonMotion> motions = new List<MenuButtonMotion>();
    private TextMeshProUGUI soundStateText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu" || scene.name == "Menu2" || scene.name == "Settings" || scene.name == "Guide")
        {
            new GameObject("Menu Polish").AddComponent<MenuPolish>();
        }
    }

    private void Start()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        TextMeshProUGUI template = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include).FirstOrDefault();
        if (canvas == null || template == null)
        {
            return;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        string scene = SceneManager.GetActiveScene().name;
        if (scene == "MainMenu")
        {
            PolishMainMenu(canvas, template);
        }
        else if (scene == "Menu2")
        {
            PolishMenu(canvas, template);
        }
        else if (scene == "Settings")
        {
            PolishSettings(canvas, template);
        }
        else
        {
            PolishGuide(canvas, template);
        }
    }

    private void LateUpdate()
    {
        if (soundStateText != null)
        {
            soundStateText.text = AudioListener.volume > 0.01f ? "SOUND ON" : "SOUND OFF";
        }
    }

    private void PolishMainMenu(Canvas canvas, TextMeshProUGUI template)
    {
        Button play = FindButton("PlayButton");
        if (play != null)
        {
            SetButtonLayout(play, new Vector2(0f, -350f), new Vector2(760f, 220f), new Color(0.72f, 0.30f, 0.055f), "PLAY NOW", "START AN ENDLESS RUN");
        }

        AddHint(canvas, template, "BEGIN YOUR ENDLESS KITCHEN RUN", new Vector2(0f, -490f), 25f);
    }

    private void PolishMenu(Canvas canvas, TextMeshProUGUI template)
    {
        RectTransform root = (RectTransform)canvas.transform;
        Image shade = CreatePanel(root, "Menu Shade", new Color(0.12f, 0.045f, 0.015f, 0.58f), new Vector2(0f, -10f), new Vector2(960f, 970f));
        shade.rectTransform.SetSiblingIndex(1);

        CreateHeading(root, template, "CHOOSE YOUR STATION", "Keep cooking, tune the kitchen, or learn the ropes");

        SetButtonLayout(FindButton("Levels_Button"), new Vector2(0f, 205f), new Vector2(780f, 205f), new Color(0.72f, 0.30f, 0.055f), "LEVELS", "CHOOSE A KITCHEN");
        SetButtonLayout(FindButton("SettingButton"), new Vector2(0f, -30f), new Vector2(780f, 205f), new Color(0.25f, 0.43f, 0.22f), "SETTINGS", "AUDIO & OPTIONS");
        SetButtonLayout(FindButton("GuideButton"), new Vector2(0f, -265f), new Vector2(780f, 205f), new Color(0.20f, 0.36f, 0.52f), "GUIDE", "LEARN HOW TO PLAY");
    }

    private void PolishSettings(Canvas canvas, TextMeshProUGUI template)
    {
        RectTransform root = (RectTransform)canvas.transform;
        Image shade = CreatePanel(root, "Settings Shade", new Color(0.12f, 0.045f, 0.015f, 0.55f), new Vector2(0f, -20f), new Vector2(980f, 820f));
        shade.rectTransform.SetSiblingIndex(1);
        TextMeshProUGUI hint = CreateText(template, root, "Settings Hint", new Vector2(0f, 230f), new Vector2(900f, 48f), 24f);
        hint.text = "SET THE MOOD BEFORE THE NEXT RUN";
        hint.color = new Color(1f, 0.94f, 0.81f);

        SetButtonLayout(FindButton("SoundButton"), new Vector2(0f, -20f), new Vector2(760f, 225f), new Color(0.25f, 0.43f, 0.22f), "SOUND ON", "CLICK TO TOGGLE");
        soundStateText = GameObject.Find("SoundButton")?.transform.Find("Polished Label")?.GetComponent<TextMeshProUGUI>();
        SetButtonLayout(FindButton("BackButton"), new Vector2(0f, -320f), new Vector2(510f, 155f), new Color(0.52f, 0.20f, 0.055f), "BACK", "RETURN TO MENU");
    }

    private void PolishGuide(Canvas canvas, TextMeshProUGUI template)
    {
        RectTransform root = (RectTransform)canvas.transform;
        TextMeshProUGUI title = CreateText(template, root, "Guide Title", new Vector2(0f, 430f), new Vector2(1000f, 75f), 48f);
        title.text = "HOW TO SURVIVE THE KITCHEN";
        TextMeshProUGUI hint = CreateText(template, root, "Guide Hint", new Vector2(0f, -330f), new Vector2(1400f, 48f), 23f);
        hint.text = "MOVE FAST  •  CATCH GOOD FOOD  •  DODGE THE ROTTEN STUFF";
        hint.color = new Color(1f, 0.86f, 0.52f);
        SetButtonLayout(FindButton("BackButton"), new Vector2(0f, -445f), new Vector2(470f, 135f), new Color(0.52f, 0.20f, 0.055f), "BACK", "RETURN TO MENU");
    }

    private void CreateHeading(RectTransform root, TextMeshProUGUI template, string titleText, string subtitleText)
    {
        TextMeshProUGUI title = CreateText(template, root, "Menu Title", new Vector2(0f, 440f), new Vector2(1100f, 82f), 54f);
        title.text = titleText;
        title.color = new Color(1f, 0.86f, 0.48f);
        TextMeshProUGUI subtitle = CreateText(template, root, "Menu Subtitle", new Vector2(0f, 378f), new Vector2(1150f, 45f), 23f);
        subtitle.text = subtitleText;
        subtitle.color = new Color(1f, 0.94f, 0.81f);
    }

    private void AddHint(Canvas canvas, TextMeshProUGUI template, string value, Vector2 position, float size)
    {
        TextMeshProUGUI hint = CreateText(template, canvas.transform, "Menu Hint", position, new Vector2(950f, 50f), size);
        hint.text = value;
        hint.color = new Color(1f, 0.86f, 0.52f);
    }

    private void SetButtonLayout(Button button, Vector2 position, Vector2 size, Color color, string label, string description)
    {
        if (button == null)
        {
            return;
        }

        RectTransform rect = (RectTransform)button.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
        rect.SetAsLastSibling();

        Image image = button.GetComponent<Image>();
        if (image != null && image.sprite != roundedSprite)
        {
            Sprite originalSprite = image.sprite;
            Color originalColor = image.color;
            Material originalMaterial = image.material;

            GameObject artObject = new GameObject("Original Button Art", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform artRect = (RectTransform)artObject.transform;
            artRect.SetParent(rect, false);
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = new Vector2(62f, 35f);
            artRect.offsetMax = new Vector2(-62f, -35f);
            Image art = artObject.GetComponent<Image>();
            art.sprite = originalSprite;
            art.color = originalColor;
            art.material = originalMaterial;
            art.preserveAspect = true;
            art.raycastTarget = false;
            artObject.SetActive(false);

            SettingsManager settings = FindAnyObjectByType<SettingsManager>();
            if (settings != null && settings.soundIcon == image)
            {
                settings.soundIcon = art;
            }

            image.sprite = GetRoundedSprite();
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = color;
            image.raycastTarget = true;
        }

        TextMeshProUGUI template = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include).FirstOrDefault();
        if (template != null)
        {
            TextMeshProUGUI title = CreateText(template, rect, "Polished Label", new Vector2(0f, 20f), new Vector2(size.x - 90f, 72f), size.y >= 190f ? 50f : 38f);
            title.text = label;
            title.color = new Color(1f, 0.91f, 0.62f);
            TextMeshProUGUI detail = CreateText(template, rect, "Polished Detail", new Vector2(0f, -44f), new Vector2(size.x - 80f, 38f), size.y >= 190f ? 21f : 17f);
            detail.text = description;
            detail.color = new Color(1f, 1f, 1f, 0.78f);
        }

        Shadow shadow = button.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = button.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0.08f, 0.025f, 0.005f, 0.72f);
        shadow.effectDistance = new Vector2(0f, -12f);

        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(1f, 0.74f, 0.30f, 0.85f);
        outline.effectDistance = new Vector2(4f, -4f);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.fadeDuration = 0.08f;
        colors.colorMultiplier = 1f;
        button.colors = colors;

        MenuButtonMotion motion = button.gameObject.GetComponent<MenuButtonMotion>();
        if (motion == null)
        {
            motion = button.gameObject.AddComponent<MenuButtonMotion>();
        }
        motions.Add(motion);
    }

    private static Button FindButton(string name)
    {
        return FindObjectsByType<Button>(FindObjectsInactive.Include).FirstOrDefault(button => button.name == name);
    }

    private static Image CreatePanel(RectTransform parent, string name, Color color, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = obj.GetComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(TextMeshProUGUI template, Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.font = template.font;
        text.fontSharedMaterial = template.fontSharedMaterial;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
        {
            return roundedSprite;
        }

        const int size = 64;
        const int radius = 14;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "Runtime Rounded Button";
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = x < radius ? radius : x >= size - radius ? size - radius - 1 : x;
                float cy = y < radius ? radius : y >= size - radius ? size - radius - 1 : y;
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                pixels[y * size + x] = new Color(1f, 1f, 1f, distance <= radius ? 1f : 0f);
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        roundedSprite.name = "Runtime Rounded Sprite";
        return roundedSprite;
    }
}

public sealed class MenuButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 targetScale = Vector3.one;

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 14f);
    }

    public void OnPointerEnter(PointerEventData eventData) => targetScale = Vector3.one * 1.045f;
    public void OnPointerExit(PointerEventData eventData) => targetScale = Vector3.one;
    public void OnPointerDown(PointerEventData eventData) => targetScale = Vector3.one * 0.97f;
    public void OnPointerUp(PointerEventData eventData) => targetScale = eventData.pointerCurrentRaycast.gameObject == gameObject ? Vector3.one * 1.045f : Vector3.one;
}
