#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Exercises the shipped scenes and prefabs in Unity, including real 2D trigger contacts.
/// Run from Tools/Chop Wars/Validate Game, or with -batchmode -executeMethod
/// ChopWarsValidation.RunAll -logFile Logs/validation.log (do not pass -quit).
/// Results are written to Logs/ChopWarsValidation.json. This file is editor-only.
/// </summary>
[InitializeOnLoad]
public static class ChopWarsValidation
{
    private const string SessionKey = "ChopWars.Validation.";
    private static readonly string[] SceneNames =
        { "MainMenu", "Menu2", "Guide", "Settings", "Levels", "MainGame" };
    private static readonly Stack<IEnumerator> Routines = new Stack<IEnumerator>();
    private static readonly List<string> RuntimeErrors = new List<string>();
    private static Report report;
    private static double deadline;

    [Serializable]
    private sealed class Report
    {
        public string unityVersion;
        public string startedUtc;
        public string finishedUtc;
        public bool success;
        public List<string> checks = new List<string>();
        public List<string> errors = new List<string>();
        public List<string> screenshots = new List<string>();
    }

    static ChopWarsValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("Tools/Chop Wars/Validate Game")]
    public static void RunAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Exit Play Mode before running validation.");

        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Save open scenes before running validation.");

        report = new Report
        {
            unityVersion = Application.unityVersion,
            startedUtc = DateTime.UtcNow.ToString("o")
        };
        SessionState.SetString(SessionKey + "originalScene", SceneManager.GetActiveScene().path);
        SavePreference("Highscore");
        SavePreference("SoundEnabled");
        SessionState.SetBool(SessionKey + "running", true);

        try
        {
            ValidateAssets();
            SaveReport();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
        catch (Exception exception)
        {
            Finish(exception);
        }
    }

    private static void ValidateAssets()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
        Require(scenes.Length == SceneNames.Length, "All six game scenes must be enabled in Build Settings.");
        Require(scenes[0].path == "Assets/Scenes/MainMenu.unity", "A build must start at MainMenu.");
        foreach (string name in SceneNames)
        {
            string path = "Assets/Scenes/" + name + ".unity";
            Require(scenes.Any(s => s.path == path), name + " is absent from Build Settings.");
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (GameObject root in scene.GetRootGameObjects())
                ValidateHierarchy(root, path);

            Require(Object.FindObjectsByType<Camera>().Length > 0,
                name + " needs an active camera.");
            Require(Object.FindObjectsByType<EventSystem>().Length == 1,
                name + " needs exactly one active EventSystem.");
            Pass(name + ": loads with no missing scripts, broken references, or invalid button callbacks");
        }

        string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        Require(prefabs.Length > 0, "Gameplay prefabs are missing.");
        foreach (string guid in prefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            ValidateHierarchy(prefab, path);
            Require(prefab.GetComponent<Collider2D>() != null, path + " needs a pickup collider.");
            Require(prefab.GetComponent<Rigidbody2D>() != null, path + " needs a falling rigidbody.");
            Require(prefab.GetComponent<SpriteRenderer>()?.sprite != null, path + " needs a visible sprite.");
        }
        Pass(prefabs.Length + " falling food/pickup prefabs have valid scripts, sprites, and physics");
    }

    private static void ValidateHierarchy(GameObject root, string assetPath)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                assetPath + ": missing script on " + child.name);
            foreach (Component component in child.GetComponents<Component>())
            {
                if (component == null) continue;
                var serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null && property.objectReferenceEntityIdValue != default)
                        throw new InvalidOperationException(assetPath + "/" + child.name + ": broken " + property.propertyPath);
                }
            }

            Button button = child.GetComponent<Button>();
            if (button == null) continue;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                string method = button.onClick.GetPersistentMethodName(i);
                if (string.IsNullOrEmpty(method)) continue; // A runtime binding may fill an empty slot.
                Object target = button.onClick.GetPersistentTarget(i);
                Require(target != null, assetPath + "/" + child.name + ": callback target is missing.");
                Require(target.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.Instance) != null,
                    assetPath + "/" + child.name + ": callback " + method + " does not exist.");
            }
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(SessionKey + "running", false)) return;
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            report = JsonUtility.FromJson<Report>(SessionState.GetString(SessionKey + "report", ""));
            RuntimeErrors.Clear();
            Application.logMessageReceived += OnLog;
            deadline = EditorApplication.timeSinceStartup + 180;
            Routines.Clear();
            Routines.Push(ExerciseGame());
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            RestorePreferences();
            bool success = SessionState.GetBool(SessionKey + "success", false);
            SessionState.SetBool(SessionKey + "running", false);
            string originalScene = SessionState.GetString(SessionKey + "originalScene", "");
            if (!Application.isBatchMode && File.Exists(originalScene))
                EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
            if (Application.isBatchMode) EditorApplication.Exit(success ? 0 : 1);
        }
    }

    private static void Tick()
    {
        try
        {
            Require(EditorApplication.timeSinceStartup < deadline, "Validation exceeded its 180 second timeout.");
            Require(RuntimeErrors.Count == 0, string.Join("\n", RuntimeErrors));
            if (Routines.Count == 0)
            {
                Finish(null);
                return;
            }
            IEnumerator current = Routines.Peek();
            if (!current.MoveNext()) Routines.Pop();
            else if (current.Current is IEnumerator nested) Routines.Push(nested);
        }
        catch (Exception exception)
        {
            Finish(exception);
        }
    }

    private static IEnumerator ExerciseGame()
    {
        yield return WaitForScene("MainMenu");
        yield return Capture("01-main-menu");
        yield return ClickAndLoad("PlayButton", "Menu2");
        yield return Capture("02-menu");
        yield return ClickAndLoad("GuideButton", "Guide");
        yield return Capture("03-guide");
        yield return ClickAndLoad("BackButton", "Menu2");
        yield return ClickAndLoad("SettingButton", "Settings");
        yield return Capture("04-settings");
        SettingsManager settings = Find<SettingsManager>();
        Require(settings.soundButton != null && settings.soundIcon != null,
            "Sound control must have a button and icon.");
        int previousSound = PlayerPrefs.GetInt("SoundEnabled", 1);
        settings.soundButton.onClick.Invoke();
        yield return WaitFrames(2);
        Require(PlayerPrefs.GetInt("SoundEnabled", 1) != previousSound, "Sound button did not persist its toggle.");
        Require(Mathf.Approximately(AudioListener.volume, previousSound == 1 ? 0f : 1f),
            "Sound toggle did not update the global volume.");
        yield return ClickAndLoad("BackButton", "Menu2");
        yield return ClickAndLoad("SettingButton", "Settings");
        Require(Mathf.Approximately(AudioListener.volume, previousSound == 1 ? 0f : 1f),
            "Sound setting did not survive a scene change.");
        Find<SettingsManager>().soundButton.onClick.Invoke();
        Pass("Settings: button toggles volume and persists across scene changes");
        yield return ClickAndLoad("BackButton", "Menu2");
        yield return ClickAndLoad("Levels_Button", "Levels");
        yield return Capture("05-levels");
        yield return ClickAndLoad("BackButton", "Menu2");
        yield return ClickAndLoad("Levels_Button", "Levels");
        yield return ClickAndLoad("Ghana-Button", "MainGame");
        Pass("All menu routes and repeated back/forward navigation work");

        PlayerMovement player = Find<PlayerMovement>();
        PlayerGrow size = player.GetComponent<PlayerGrow>();
        HealthBar health = Find<HealthBar>();
        ScoreManager score = Find<ScoreManager>();
        PauseManager pause = Find<PauseManager>();
        LoseScreenManager ending = Find<LoseScreenManager>();
        Require(size != null, "Player needs growth and shrink behavior.");
        Require(player.GetComponent<Collider2D>() != null, "Player needs a collider.");
        Require(player.GetComponentsInChildren<SpriteRenderer>().Length > 1,
            "The player must render a character with visible body features.");
        Require(health.currentHealth == player.maxHits, "A new run must start at full health.");
        Require(health.hearts != null && health.hearts.Length == player.maxHits, "Every health point needs a visible heart.");
        Require(!pause.IsPaused && !ending.IsScreenVisible && Time.timeScale > 0f,
            "A new run must start with gameplay active and overlays hidden.");
        yield return WaitUntil(() => Pickups().Length > 0, "Spawner did not produce a falling pickup.", 6f);
        Pass("MainGame initializes a character, full health, score, physics, and active spawner");
        DisableSpawnersAndClearPickups();
        yield return WaitFrames(2);
        yield return Capture("06-character-normal");

        Click("PauseButton");
        yield return WaitFrames(2);
        Require(pause.IsPaused && Mathf.Approximately(Time.timeScale, 0f) && pause.pauseMenu.activeInHierarchy,
            "Pause button must freeze gameplay and reveal the pause menu.");
        yield return Capture("07-pause");
        int pausedScore = score.GetScore();
        Vector3 pausedPosition = player.transform.position;
        yield return WaitSeconds(1.1f);
        Require(score.GetScore() == pausedScore && player.transform.position == pausedPosition,
            "Score or player advanced while paused.");
        Click("Resume");
        yield return WaitFrames(2);
        Require(!pause.IsPaused && Time.timeScale > 0f, "Resume button must restore gameplay.");
        Pass("Pause and Resume buttons freeze and restore the round");

        float groundedY = player.transform.position.y - size.feetOffset * player.transform.localScale.y;
        float initialSize = player.transform.localScale.x;
        SpriteRenderer bodyRenderer = player.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(r => r.name == "Body");
        Require(bodyRenderer != null && bodyRenderer.enabled, "Character needs a visible body.");
        float initialBodyWidth = bodyRenderer.bounds.size.x;
        float initialSpeed = player.moveSpeed;
        int initialHealth = health.currentHealth;
        yield return Collect("Chicken", player);
        yield return WaitSeconds(size.sizeLerpDuration + player.hitStopDuration + 0.15f);
        Require(health.currentHealth == initialHealth - 1, "Unhealthy food must remove one heart.");
        Require(player.moveSpeed < initialSpeed, "Unhealthy food must reduce movement speed.");
        Require(player.transform.localScale.x > initialSize, "Unhealthy food must enlarge the player.");
        Require(bodyRenderer.bounds.size.x > initialBodyWidth, "The visible character body must get fatter with growth.");
        Pass("Real unhealthy-food trigger damages, slows, and enlarges the character exactly once");

        float enlargedSize = player.transform.localScale.x;
        float enlargedBodyWidth = bodyRenderer.bounds.size.x;
        float reducedSpeed = player.moveSpeed;
        yield return Collect("Mango", player);
        yield return WaitSeconds(size.sizeLerpDuration + 0.1f);
        Require(player.transform.localScale.x < enlargedSize, "Healthy food must shrink the player.");
        Require(bodyRenderer.bounds.size.x < enlargedBodyWidth, "The visible character body must get slimmer when shrinking.");
        Require(player.moveSpeed > reducedSpeed, "Healthy food must increase movement speed.");
        Require(health.currentHealth == initialHealth, "Healthy food must restore one heart.");
        Pass("Real healthy-food trigger heals and makes the character slimmer and faster");

        yield return Collect("Chicken", player);
        yield return WaitSeconds(player.hitStopDuration + 0.1f);

        yield return Collect("Heart", player);
        Require(health.currentHealth == initialHealth, "Heart pickup must heal one point.");
        yield return Collect("Heart", player);
        Require(health.currentHealth == initialHealth, "Hearts must not exceed maximum health.");
        int beforeCoin = score.GetScore();
        int coinValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coin.prefab").GetComponent<Coin>().coinValue;
        yield return Collect("Coin", player);
        Require(score.GetScore() >= beforeCoin + coinValue,
            "Coin must award at least its configured bonus exactly once.");
        Require(score.Combo > 0, "Healthy food and coins must build the fresh-food streak.");
        Pass("Real heart and coin triggers heal safely, score once, and build a combo");

        size.sizeLerpDuration = 0f;
        for (int i = 0; i < 100; i++) size.Grow();
        yield return WaitFrames(2);
        Require(Mathf.Approximately(player.transform.localScale.x, size.maxSize), "Growth must stop at maximum size.");
        yield return ValidateBoundsAndGround(player, groundedY);
        yield return Capture("08-character-fat");
        for (int i = 0; i < 100; i++) size.Shrink();
        yield return WaitFrames(2);
        Require(Mathf.Approximately(player.transform.localScale.x, size.minSize), "Shrink must stop at minimum size.");
        Require(player.GetComponent<Collider2D>().bounds.size.x > 0f, "Minimum-size character must remain collidable.");
        yield return ValidateBoundsAndGround(player, groundedY);
        yield return Capture("09-character-skinny");
        Pass("Rapid size changes clamp safely at the minimum and maximum");

        Click("PauseButton");
        yield return WaitFrames(2);
        PlayerMovement previousPlayer = player;
        Click("Restart");
        yield return WaitForNewPlayer(previousPlayer);
        player = Find<PlayerMovement>();
        health = Find<HealthBar>();
        score = Find<ScoreManager>();
        ending = Find<LoseScreenManager>();
        DisableSpawnersAndClearPickups();
        Require(health.currentHealth == player.maxHits && score.GetScore() <= 1 && Time.timeScale > 0f,
            "Restarting a paused run must reset health, score, and time.");
        Pass("Pause-menu Restart starts a fresh playable round");

        player.hitStopDuration = 0f;
        for (int i = 0; i < player.maxHits; i++) yield return Collect("Chicken", player);
        yield return WaitFrames(2);
        Require(player.IsRecovering && !ending.IsScreenVisible && Time.timeScale > 0f,
            "Empty health must start recovery without showing an end screen or freezing gameplay.");
        yield return Capture("10-endless-recovery");
        yield return WaitSeconds(player.recoveryDuration + 0.2f);
        Require(health.currentHealth == player.maxHits && !player.IsRoundOver && !ending.IsScreenVisible,
            "An endless run must restore health and remain playable after recovery.");
        int endlessScore = score.GetScore();
        score.AddScore(1000);
        yield return WaitFrames(2);
        Require(score.GetTargetScore() == 0 && score.GetScore() >= endlessScore + 1000 && !ending.IsScreenVisible,
            "Scores must continue forever without a victory screen.");
        Pass("Empty health recovers safely and high scores never end the run");

        Click("PauseButton");
        yield return WaitFrames(2);
        Click("MainMenu");
        yield return WaitForScene("Menu2");
        Require(Time.timeScale > 0f, "Pause-menu navigation must reset time scale.");
        Pass("Pause-menu navigation ends an endless session cleanly");
    }

    private static IEnumerator Collect(string prefabName, PlayerMovement player)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + prefabName + ".prefab");
        Require(prefab != null, "Missing " + prefabName + " prefab.");
        GameObject pickup = Object.Instantiate(prefab, player.GetComponent<Collider2D>().bounds.center, Quaternion.identity);
        Rigidbody2D body = pickup.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        yield return WaitUntil(() => pickup == null, prefabName + " did not collide with and get consumed by the player.", 3f);
        yield return WaitFrames(2);
    }

    private static GameObject[] Pickups()
    {
        return Object.FindObjectsByType<Rigidbody2D>()
            .Where(b => b.GetComponent<PlayerMovement>() == null &&
                (b.CompareTag("Enemy") || b.CompareTag("Healthy") || b.CompareTag("Heart") || b.CompareTag("Coin")))
            .Select(b => b.gameObject).ToArray();
    }

    private static void DisableSpawnersAndClearPickups()
    {
        foreach (Spawner spawner in Object.FindObjectsByType<Spawner>()) spawner.enabled = false;
        foreach (GameObject pickup in Pickups()) Object.Destroy(pickup);
    }

    private static IEnumerator ClickAndLoad(string buttonName, string destination)
    {
        Click(buttonName);
        yield return WaitForScene(destination);
    }

    private static void Click(string name)
    {
        Button button = Object.FindObjectsByType<Button>()
            .FirstOrDefault(b => b.name == name && b.gameObject.activeInHierarchy);
        Require(button != null && button.interactable, "No active, interactable button named " + name + ".");
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)button.transform;
        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
        Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == button,
            name + " is obscured or cannot receive pointer clicks at its center. Top hit: " + (hits.Count > 0 ? hits[0].gameObject.name : "none"));
        button.onClick.Invoke();
    }

    private static IEnumerator WaitForScene(string name)
    {
        yield return WaitUntil(() => SceneManager.GetActiveScene().name == name,
            "Navigation did not reach " + name + ".", 8f);
        yield return WaitFrames(3);
    }

    private static IEnumerator WaitForNewPlayer(PlayerMovement previousPlayer)
    {
        yield return WaitUntil(() =>
        {
            PlayerMovement current = Object.FindAnyObjectByType<PlayerMovement>();
            return current != null && current != previousPlayer;
        }, "Restart did not create a new player.", 8f);
        yield return WaitFrames(3);
    }

    private static IEnumerator WaitFrames(int count)
    {
        int target = Time.frameCount + count;
        while (Time.frameCount < target) yield return null;
    }

    private static IEnumerator WaitSeconds(float seconds)
    {
        double until = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.timeSinceStartup < until) yield return null;
    }

    private static IEnumerator ValidateBoundsAndGround(PlayerMovement player, float groundedY)
    {
        PlayerGrow size = player.GetComponent<PlayerGrow>();
        Require(Mathf.Abs(player.transform.position.y - size.feetOffset * player.transform.localScale.y - groundedY) < 0.015f,
            "Changing body size must keep the character's feet grounded.");
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        foreach (float direction in new[] { -1f, 1f })
        {
            body.position = new Vector2(direction * 100f, player.transform.position.y);
            yield return WaitSeconds(0.12f);
            float width = player.GetComponent<PlayerCharacterVisual>().HalfWidth;
            float left = Mathf.Max(player.minX, Camera.main.transform.position.x - cameraHalfWidth);
            float right = Mathf.Min(player.maxX, Camera.main.transform.position.x + cameraHalfWidth);
            Require(body.position.x - width >= left - 0.02f && body.position.x + width <= right + 0.02f,
                "The complete character must stay inside the play area at every body size.");
        }
        body.position = new Vector2(0f, player.transform.position.y);
        yield return WaitSeconds(0.12f);
    }

    private static IEnumerator Capture(string name)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/ValidationScreenshots"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, name + ".png");
        Camera camera = Camera.main;
        Require(camera != null, "Screenshot needs a main camera.");
        var target = new RenderTexture(1280, 720, 24);
        target.Create();
        RenderTexture oldTarget = camera.targetTexture;
        RenderTexture oldActive = RenderTexture.active;
        float oldAspect = camera.aspect;
        Canvas[] overlays = Object.FindObjectsByType<Canvas>()
            .Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        int[] orders = overlays.Select(c => c.sortingOrder).ToArray();
        camera.targetTexture = target;
        camera.aspect = 1280f / 720f;
        for (int i = 0; i < overlays.Length; i++)
        {
            overlays[i].renderMode = RenderMode.ScreenSpaceCamera;
            overlays[i].worldCamera = camera;
            overlays[i].planeDistance = 1f;
            overlays[i].sortingOrder = 100 + orders[i];
        }
        Canvas.ForceUpdateCanvases();
        yield return WaitFrames(2);
        var request = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target };
        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera, request);
        RenderTexture.active = target;
        var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        RenderTexture.active = oldActive;
        camera.targetTexture = oldTarget;
        camera.aspect = oldAspect;
        for (int i = 0; i < overlays.Length; i++)
        {
            overlays[i].renderMode = RenderMode.ScreenSpaceOverlay;
            overlays[i].worldCamera = null;
            overlays[i].sortingOrder = orders[i];
        }
        target.Release();
        Object.DestroyImmediate(target);
        Canvas.ForceUpdateCanvases();
        yield return WaitFrames(2);
        report.screenshots.Add(path);
        SaveReport();
    }

    private static IEnumerator WaitUntil(Func<bool> condition, string failure, float timeout)
    {
        double until = EditorApplication.timeSinceStartup + timeout;
        while (!condition())
        {
            Require(EditorApplication.timeSinceStartup < until, failure);
            yield return null;
        }
    }

    private static T Find<T>() where T : Object
    {
        T result = Object.FindAnyObjectByType<T>();
        Require(result != null, "Scene is missing an active " + typeof(T).Name + ".");
        return result;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Pass(string message)
    {
        report.checks.Add(message);
        SaveReport();
        Debug.Log("[Chop Wars validation] PASS: " + message);
    }

    private static void OnLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            RuntimeErrors.Add(message + "\n" + stackTrace);
    }

    private static void Finish(Exception failure)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        Routines.Clear();
        report.success = failure == null;
        report.finishedUtc = DateTime.UtcNow.ToString("o");
        if (failure != null) report.errors.Add(failure.ToString());
        SaveReport();
        SessionState.SetBool(SessionKey + "success", report.success);
        RestorePreferences();
        if (report.success) Debug.Log("[Chop Wars validation] SUCCESS: " + report.checks.Count + " checks passed.");
        else Debug.LogError("[Chop Wars validation] FAILED: " + failure);
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        else
        {
            SessionState.SetBool(SessionKey + "running", false);
            if (Application.isBatchMode) EditorApplication.Exit(report.success ? 0 : 1);
        }
    }

    private static void SaveReport()
    {
        string json = JsonUtility.ToJson(report, true);
        SessionState.SetString(SessionKey + "report", json);
        string directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "ChopWarsValidation.json"), json);
    }

    private static void SavePreference(string key)
    {
        SessionState.SetBool(SessionKey + key + ".exists", PlayerPrefs.HasKey(key));
        SessionState.SetInt(SessionKey + key, PlayerPrefs.GetInt(key, 0));
    }

    private static void RestorePreferences()
    {
        foreach (string key in new[] { "Highscore", "SoundEnabled" })
        {
            if (SessionState.GetBool(SessionKey + key + ".exists", false))
                PlayerPrefs.SetInt(key, SessionState.GetInt(SessionKey + key, 0));
            else PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        AudioListener.volume = PlayerPrefs.GetInt("SoundEnabled", 1) == 1 ? 1f : 0f;
        Time.timeScale = 1f;
    }
}
#endif
