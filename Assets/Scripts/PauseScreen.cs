using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenu;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu2");
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;

        // ✅ Reset score if you have ScoreManager
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            // clears current score, keeps high score
            typeof(ScoreManager).GetField("score", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(scoreManager, 0);
        }

        // ✅ Reset health if HealthBar exists
        HealthBar healthBar = FindObjectOfType<HealthBar>();
        if (healthBar != null)
        {
            healthBar.ResetHealth();
        }

        // ✅ Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
