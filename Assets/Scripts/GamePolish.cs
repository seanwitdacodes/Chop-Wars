using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Adds lightweight presentation polish without requiring duplicated UI wiring in every scene.
/// </summary>
public sealed class GamePolish : MonoBehaviour
{
    private static Sprite solidSprite;
    private ScoreManager score;
    private PlayerMovement player;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI comboText;
    private TextMeshProUGUI recoveryText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        string scene = loadedScene.name;
        if (scene != "MainGame" && scene != "Levels")
        {
            return;
        }

        new GameObject("Game Polish").AddComponent<GamePolish>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Levels")
        {
            PolishLevelsScreen();
        }
        else
        {
            PolishGameplay();
        }
    }

    private void Update()
    {
        if (score == null)
        {
            return;
        }

        if (highScoreText != null)
        {
            highScoreText.text = $"HIGH SCORE  {score.GetHighScore():0000}";
        }

        if (comboText != null)
        {
            comboText.gameObject.SetActive(score.Combo > 0);
            comboText.text = score.Multiplier > 1
                ? $"FRESH STREAK {score.Combo}   ×{score.Multiplier}"
                : $"FRESH STREAK {score.Combo}";
        }

        if (recoveryText != null && player != null)
        {
            recoveryText.gameObject.SetActive(player.IsRecovering);
        }
    }

    private void PolishGameplay()
    {
        score = FindAnyObjectByType<ScoreManager>();
        player = FindAnyObjectByType<PlayerMovement>();
        TextMeshProUGUI template = score != null ? score.scoreText : FindAnyObjectByType<TextMeshProUGUI>();

        Transform floor = GameObject.Find("Bottom Wall")?.transform;
        SpriteRenderer floorRenderer = floor != null ? floor.GetComponent<SpriteRenderer>() : null;
        if (floorRenderer != null)
        {
            Sprite counterSprite = GetSolidSprite();
            floorRenderer.sprite = counterSprite;
            floorRenderer.color = new Color(0.36f, 0.16f, 0.055f, 1f);
            floorRenderer.sortingOrder = 2;
            AddFloorLayer(floor, counterSprite, "Counter Highlight", new Color(0.91f, 0.56f, 0.20f), 0.34f, 0.13f, 3);
            AddFloorLayer(floor, counterSprite, "Counter Edge", new Color(0.13f, 0.055f, 0.025f), -0.30f, 0.18f, 3);
            for (int i = -3; i <= 3; i += 2)
            {
                GameObject seam = new GameObject("Plank Seam");
                seam.transform.SetParent(floor, false);
                seam.transform.localPosition = new Vector3(i * 0.125f, 0f, -0.01f);
                seam.transform.localScale = new Vector3(0.007f, 0.72f, 1f);
                SpriteRenderer seamRenderer = seam.AddComponent<SpriteRenderer>();
                seamRenderer.sprite = counterSprite;
                seamRenderer.color = new Color(0.18f, 0.07f, 0.025f, 0.75f);
                seamRenderer.sortingOrder = 3;
            }
        }

        if (player != null)
        {
            SpriteRenderer source = player.GetComponent<SpriteRenderer>();
            if (source != null)
            {
                GameObject shadow = new GameObject("Ground Shadow");
                shadow.transform.SetParent(player.transform, false);
                shadow.transform.localPosition = new Vector3(0f, -0.92f, 0f);
                shadow.transform.localScale = new Vector3(1.25f, 0.16f, 1f);
                SpriteRenderer renderer = shadow.AddComponent<SpriteRenderer>();
                renderer.sprite = source.sprite;
                renderer.color = new Color(0f, 0f, 0f, 0.28f);
                renderer.sortingOrder = -2;
            }
        }

        if (template == null)
        {
            return;
        }

        Transform canvas = template.transform.parent;
        Image plaque = CreatePanel((RectTransform)canvas, "Score Plaque", new Color(0.86f, 0.57f, 0.22f), new Vector2(0f, -112f), new Vector2(560f, 180f));
        plaque.rectTransform.anchorMin = plaque.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        plaque.rectTransform.SetAsFirstSibling();
        plaque.sprite = MenuPolish.GetRoundedSprite();
        plaque.type = Image.Type.Sliced;
        Image inset = CreatePanel(plaque.rectTransform, "Dark Wood", new Color(0.19f, 0.085f, 0.025f), Vector2.zero, new Vector2(548f, 168f));
        inset.sprite = MenuPolish.GetRoundedSprite();
        inset.type = Image.Type.Sliced;
        CreatePanel(plaque.rectTransform, "Score Divider", new Color(0.86f, 0.57f, 0.22f, 0.6f), new Vector2(0f, 8f), new Vector2(460f, 2f));

        template.rectTransform.anchorMin = template.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        template.rectTransform.anchoredPosition = new Vector2(0f, -66f);
        template.rectTransform.sizeDelta = new Vector2(470f, 66f);
        template.alignment = TextAlignmentOptions.Center;
        template.enableAutoSizing = true;
        template.fontSizeMin = 26f;
        template.fontSizeMax = 52f;
        highScoreText = CreateText(template, canvas, "High Score", new Vector2(0.5f, 1f), new Vector2(0f, -148f), new Vector2(500f, 50f), 34f, TextAlignmentOptions.Center);
        highScoreText.color = new Color(1f, 0.89f, 0.61f);
        highScoreText.enableAutoSizing = true;
        highScoreText.fontSizeMin = 24f;
        highScoreText.fontSizeMax = 34f;
        highScoreText.text = $"HIGH SCORE  {score.GetHighScore():0000}";
        comboText = CreateText(template, canvas, "Combo", new Vector2(0.5f, 1f), new Vector2(0f, -234f), new Vector2(560f, 56f), 30f, TextAlignmentOptions.Center);
        comboText.color = new Color(0.16f, 0.26f, 0.065f);
        recoveryText = CreateText(template, canvas, "Recovery", new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(850f, 110f), 52f, TextAlignmentOptions.Center);
        recoveryText.text = "BACK TO THE CUTTING BOARD!";
        recoveryText.color = new Color(1f, 0.82f, 0.25f);
        recoveryText.gameObject.SetActive(false);
    }

    private static void AddFloorLayer(Transform floor, Sprite sprite, string name, Color color, float y, float height, int order)
    {
        GameObject layer = new GameObject(name);
        layer.transform.SetParent(floor, false);
        layer.transform.localPosition = new Vector3(0f, y, -0.01f);
        layer.transform.localScale = new Vector3(1.01f, height, 1f);
        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
    }

    private static Sprite GetSolidSprite()
    {
        if (solidSprite == null)
        {
            solidSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            solidSprite.name = "Runtime Solid Sprite";
        }

        return solidSprite;
    }

    private void PolishLevelsScreen()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        TextMeshProUGUI template = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include).FirstOrDefault();
        if (canvas == null || template == null)
        {
            return;
        }

        RectTransform root = canvas.transform as RectTransform;
        MenuPolish.ApplyJungleBackdrop(canvas);
        Image shade = CreatePanel(root, "Menu Shade", new Color(0.08f, 0.20f, 0.07f, 0.16f), Vector2.zero, new Vector2(1920f, 1080f));
        shade.rectTransform.SetSiblingIndex(1);

        Image card = CreatePanel(root, "Ghana Card", new Color(0.22f, 0.11f, 0.035f, 0.88f), new Vector2(0f, -20f), new Vector2(1320f, 500f));
        card.rectTransform.SetSiblingIndex(2);
        Image accent = CreatePanel(root, "Ghana Card Accent", new Color(1f, 0.70f, 0.24f, 1f), new Vector2(0f, 224f), new Vector2(1320f, 12f));
        accent.rectTransform.SetSiblingIndex(3);

        CreateText(template, root, "Title", new Vector2(0.5f, 0.5f), new Vector2(0f, 425f), new Vector2(1300f, 95f), 64f, TextAlignmentOptions.Center).text = "CHOOSE YOUR KITCHEN";
        TextMeshProUGUI subtitle = CreateText(template, root, "Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0f, 350f), new Vector2(1200f, 60f), 28f, TextAlignmentOptions.Center);
        subtitle.text = "Travel the world one endless recipe run at a time";
        subtitle.color = new Color(1f, 0.91f, 0.66f);

        Button ghana = FindObjectsByType<Button>(FindObjectsInactive.Include).FirstOrDefault(button => button.name == "Ghana-Button");
        RectTransform icon = GameObject.Find("Icon")?.transform as RectTransform;
        if (ghana != null)
        {
            RectTransform rect = (RectTransform)ghana.transform;
            rect.anchoredPosition = new Vector2(300f, 20f);
            rect.sizeDelta = new Vector2(620f, 310f);
            rect.SetAsLastSibling();
        }

        if (icon != null)
        {
            icon.localRotation = Quaternion.identity;
            icon.anchoredPosition = new Vector2(-330f, 15f);
            icon.sizeDelta = new Vector2(390f, 390f);
            icon.SetAsLastSibling();
        }

        TextMeshProUGUI mode = CreateText(template, root, "Mode", new Vector2(0.5f, 0.5f), new Vector2(300f, -145f), new Vector2(650f, 60f), 26f, TextAlignmentOptions.Center);
        mode.text = "ENDLESS  •  DODGE  •  COLLECT  •  SURVIVE";
        mode.color = new Color(1f, 0.85f, 0.42f);

        TextMeshProUGUI best = CreateText(template, root, "Best", new Vector2(0.5f, 0.5f), new Vector2(-330f, -190f), new Vector2(450f, 50f), 26f, TextAlignmentOptions.Center);
        best.text = $"PERSONAL BEST  {Mathf.Max(0, PlayerPrefs.GetInt("Highscore", 0)):0000}";
        best.color = new Color(0.80f, 1f, 0.76f);

        TextMeshProUGUI coming = CreateText(template, root, "Coming Soon", new Vector2(0.5f, 0.5f), new Vector2(0f, -330f), new Vector2(900f, 48f), 24f, TextAlignmentOptions.Center);
        coming.text = "MORE WORLD KITCHENS COMING SOON";
        coming.color = new Color(1f, 1f, 1f, 0.75f);

        Button back = FindObjectsByType<Button>(FindObjectsInactive.Include).FirstOrDefault(button => button.name == "BackButton");
        if (back != null)
        {
            MenuPolish.StyleWoodButton(back, new Vector2(-760f, -445f), new Vector2(360f, 125f),
                new Color(0.91f, 0.60f, 0.23f), "BACK", "RETURN TO MENU");
        }
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
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(TextMeshProUGUI template, Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
        text.font = template.font;
        text.fontSharedMaterial = template.fontSharedMaterial;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return text;
    }
}
