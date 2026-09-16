using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    private static bool sceneLoadRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeNavigation()
    {
        sceneLoadRequested = false;
        Time.timeScale = 1f;
        // Remove first so entering Play Mode without a domain reload is also safe.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoadRequested = false;
    }

    // These methods are connected by the scenes' serialized button callbacks.
    public void LoadMenu2() => LoadScene("Menu2");
    public void LoadGuide() => LoadScene("Guide");
    public void LoadSettings() => LoadScene("Settings");
    public void LoadLevels() => LoadScene("Levels");
    public void LoadGame() => LoadScene("MainGame");

    public static void LoadScene(string sceneName)
    {
        if (sceneLoadRequested)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"ButtonScript: Scene '{sceneName}' is not in Build Settings.");
            return;
        }

        sceneLoadRequested = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
