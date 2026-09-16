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

        ApplyJungleBackdrop(canvas);

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
            StyleWoodButton(play, new Vector2(0f, -350f), new Vector2(760f, 190f), new Color(0.91f, 0.60f, 0.23f), "PLAY NOW", "START AN ENDLESS RUN");
        }

        AddHint(canvas, template, "BEGIN YOUR ENDLESS KITCHEN RUN", new Vector2(0f, -490f), 25f);
    }

    private void PolishMenu(Canvas canvas, TextMeshProUGUI template)
    {
        RectTransform root = (RectTransform)canvas.transform;
        Image shade = CreatePanel(root, "Menu Shade", new Color(0.10f, 0.22f, 0.08f, 0.16f), new Vector2(0f, -10f), new Vector2(960f, 970f));
        shade.rectTransform.SetSiblingIndex(1);

        CreateHeading(root, template, "CHOOSE YOUR STATION", "Keep cooking, tune the kitchen, or learn the ropes");

        CreateRopes(root, 205f, -265f, 335f);

        Color wood = new Color(0.91f, 0.60f, 0.23f);
        StyleWoodButton(FindButton("Levels_Button"), new Vector2(0f, 205f), new Vector2(780f, 190f), wood, "LEVELS", "CHOOSE A KITCHEN");
        StyleWoodButton(FindButton("SettingButton"), new Vector2(0f, -30f), new Vector2(780f, 190f), wood, "SETTINGS", "AUDIO & OPTIONS");
        StyleWoodButton(FindButton("GuideButton"), new Vector2(0f, -265f), new Vector2(780f, 190f), wood, "GUIDE", "LEARN HOW TO PLAY");
    }

    private void PolishSettings(Canvas canvas, TextMeshProUGUI template)
    {
        RectTransform root = (RectTransform)canvas.transform;
        Image shade = CreatePanel(root, "Settings Shade", new Color(0.10f, 0.22f, 0.08f, 0.16f), new Vector2(0f, -20f), new Vector2(980f, 820f));
        shade.rectTransform.SetSiblingIndex(1);
        TextMeshProUGUI hint = CreateText(template, root, "Settings Hint", new Vector2(0f, 230f), new Vector2(900f, 48f), 24f);
        hint.text = "SET THE MOOD BEFORE THE NEXT RUN";
        hint.color = new Color(1f, 0.94f, 0.81f);

        StyleWoodButton(FindButton("SoundButton"), new Vector2(0f, -20f), new Vector2(760f, 195f), new Color(0.91f, 0.60f, 0.23f), "SOUND ON", "CLICK TO TOGGLE");
        soundStateText = GameObject.Find("SoundButton")?.transform.Find("Polished Label")?.GetComponent<TextMeshProUGUI>();
        StyleWoodButton(FindButton("BackButton"), new Vector2(0f, -320f), new Vector2(510f, 145f), new Color(0.91f, 0.60f, 0.23f), "BACK", "RETURN TO MENU");
    }

    private void PolishGuide(Canvas canvas, TextMeshProUGUI template)
    {
        RectTransform root = (RectTransform)canvas.transform;
        foreach (TextMeshProUGUI oldText in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
        {
            if (oldText.text.Contains("Reach 0250") || oldText.text.Contains("Move with Arrow"))
            {
                oldText.gameObject.SetActive(false);
            }
        }
        Image board = CreatePanel(root, "Guide Wood Board", new Color(0.92f, 0.60f, 0.22f, 0.97f), new Vector2(0f, 40f), new Vector2(1300f, 690f));
        Shadow boardShadow = board.gameObject.AddComponent<Shadow>();
        boardShadow.effectColor = new Color(0.15f, 0.06f, 0.015f, 0.72f);
        boardShadow.effectDistance = new Vector2(0f, -15f);
        Outline boardOutline = board.gameObject.AddComponent<Outline>();
        boardOutline.effectColor = new Color(0.28f, 0.12f, 0.035f, 1f);
        boardOutline.effectDistance = new Vector2(6f, -6f);
        AddWoodDetail(board.rectTransform, board.rectTransform.sizeDelta);

        TextMeshProUGUI title = CreateText(template, root, "Guide Title", new Vector2(0f, 310f), new Vector2(1100f, 72f), 48f);
        title.text = "HOW TO PLAY";
        title.color = new Color(0.28f, 0.12f, 0.035f);
        TextMeshProUGUI subtitle = CreateText(template, root, "Guide Subtitle", new Vector2(0f, 255f), new Vector2(1050f, 42f), 23f);
        subtitle.text = "KEEP THE CHEF MOVING AND BUILD YOUR ENDLESS STREAK";
        subtitle.color = new Color(0.38f, 0.19f, 0.07f);

        CreateGuideRow(template, root, 145f, "MOVE", "ARROW KEYS / A-D  OR  DRAG / TOUCH");
        CreateGuideRow(template, root, 35f, "GOOD FRUIT", "SPEED UP, GET SLIMMER, AND SCORE POINTS");
        CreateGuideRow(template, root, -75f, "ROTTEN FOOD", "LOSE A HEART, SLOW DOWN, AND GET BIGGER");
        CreateGuideRow(template, root, -185f, "HEART PICKUP", "RESTORES ONE HEART — FRUIT DOES NOT HEAL");

        TextMeshProUGUI pause = CreateText(template, root, "Pause Tip", new Vector2(0f, -262f), new Vector2(900f, 36f), 20f);
        pause.text = "PRESS ESC ANY TIME TO PAUSE";
        pause.color = new Color(0.33f, 0.15f, 0.05f, 0.85f);
        StyleWoodButton(FindButton("BackButton"), new Vector2(0f, -445f), new Vector2(470f, 135f), new Color(0.91f, 0.60f, 0.23f), "BACK", "RETURN TO MENU");
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

    public static void StyleWoodButton(Button button, Vector2 position, Vector2 size, Color color, string label, string description)
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

        AddWoodDetail(rect, size);

        TextMeshProUGUI template = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include).FirstOrDefault();
        if (template != null)
        {
            TextMeshProUGUI title = CreateText(template, rect, "Polished Label", new Vector2(0f, 20f), new Vector2(size.x - 90f, 72f), size.y >= 190f ? 50f : 38f);
            title.text = label;
            title.color = new Color(0.30f, 0.14f, 0.055f);
            Shadow titleShadow = title.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(1f, 0.86f, 0.53f, 0.72f);
            titleShadow.effectDistance = new Vector2(2f, -2f);
            TextMeshProUGUI detail = CreateText(template, rect, "Polished Detail", new Vector2(0f, -44f), new Vector2(size.x - 80f, 38f), size.y >= 190f ? 21f : 17f);
            detail.text = description;
            detail.color = new Color(0.34f, 0.17f, 0.07f, 0.86f);
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
        outline.effectColor = new Color(0.30f, 0.14f, 0.05f, 0.95f);
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
    }

    private static void CreateGuideRow(TextMeshProUGUI template, RectTransform root, float y, string heading, string detail)
    {
        Image badge = CreatePanel(root, heading + " Badge", new Color(0.24f, 0.43f, 0.14f, 0.96f), new Vector2(-455f, y), new Vector2(250f, 74f));
        Outline outline = badge.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.20f, 0.09f, 0.025f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);
        TextMeshProUGUI label = CreateText(template, root, heading + " Label", new Vector2(-455f, y), new Vector2(225f, 52f), 25f);
        label.text = heading;
        label.color = new Color(1f, 0.91f, 0.62f);
        TextMeshProUGUI explanation = CreateText(template, root, heading + " Detail", new Vector2(165f, y), new Vector2(900f, 55f), 25f);
        explanation.text = detail;
        explanation.alignment = TextAlignmentOptions.Left;
        explanation.color = new Color(0.28f, 0.12f, 0.035f);
    }

    private static void AddWoodDetail(RectTransform button, Vector2 size)
    {
        Image topHighlight = CreatePanel(button, "Wood Highlight", new Color(1f, 0.88f, 0.49f, 0.70f), new Vector2(0f, size.y * 0.5f - 15f), new Vector2(size.x - 38f, 11f));
        topHighlight.rectTransform.SetAsFirstSibling();
        for (int i = 0; i < 2; i++)
        {
            Image grain = CreatePanel(button, "Wood Grain", new Color(0.40f, 0.19f, 0.06f, 0.16f), new Vector2((i == 0 ? -1f : 1f) * size.x * 0.08f, i == 0 ? -4f : 37f), new Vector2(size.x * (i == 0 ? 0.64f : 0.47f), 7f));
            grain.rectTransform.SetAsFirstSibling();
        }
    }

    private static void CreateRopes(RectTransform root, float topY, float bottomY, float x)
    {
        float height = topY - bottomY + 260f;
        float centerY = (topY + bottomY) * 0.5f;
        foreach (float side in new[] { -1f, 1f })
        {
            Image dark = CreatePanel(root, "Hanging Rope", new Color(0.27f, 0.12f, 0.035f, 1f), new Vector2(side * x, centerY), new Vector2(18f, height));
            dark.rectTransform.SetSiblingIndex(2);
            Image light = CreatePanel(root, "Rope Highlight", new Color(0.83f, 0.52f, 0.20f, 1f), new Vector2(side * x - 3f, centerY), new Vector2(5f, height));
            light.rectTransform.SetSiblingIndex(3);
        }
    }

    public static void ApplyJungleBackdrop(Canvas canvas)
    {
        Texture2D texture = Resources.Load<Texture2D>("JungleMenuBackground");
        if (texture == null)
        {
            Debug.LogWarning("MenuPolish: JungleMenuBackground could not be loaded.");
            return;
        }

        RectTransform root = (RectTransform)canvas.transform;
        foreach (RectTransform child in root.Cast<Transform>().OfType<RectTransform>().ToArray())
        {
            Image image = child.GetComponent<Image>();
            if (image != null && child.GetComponent<Button>() == null &&
                child.anchorMin.x <= 0.01f && child.anchorMin.y <= 0.01f &&
                child.anchorMax.x >= 0.99f && child.anchorMax.y >= 0.99f)
            {
                child.gameObject.SetActive(false);
            }
        }

        GameObject obj = new GameObject("Jungle Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(root, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        RawImage background = obj.GetComponent<RawImage>();
        background.texture = texture;
        background.raycastTarget = false;
        rect.SetAsFirstSibling();
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
