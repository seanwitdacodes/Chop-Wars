using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenu;

    private bool isPaused;
    private LoseScreenManager loseScreenManager;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        CloseForEndScreen();
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void LoadMenu() => ButtonScript.LoadScene("Menu2");

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused || HasEndScreen() || pauseMenu == null)
        {
            return;
        }

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (HasEndScreen())
        {
            return;
        }

        CloseForEndScreen();
        Time.timeScale = 1f;
    }

    public void RestartGame() => ButtonScript.LoadScene(SceneManager.GetActiveScene().name);

    // End screens share the paused clock but must replace the pause controls.
    public void CloseForEndScreen()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        isPaused = false;
    }

    private bool HasEndScreen()
    {
        if (loseScreenManager == null)
        {
            loseScreenManager = FindAnyObjectByType<LoseScreenManager>();
        }

        return loseScreenManager != null && loseScreenManager.IsScreenVisible;
    }
}
