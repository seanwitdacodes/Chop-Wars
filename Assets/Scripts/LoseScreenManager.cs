using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loseScreen;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public Button restartButton;
    public Button menuButton;

    [Header("Messages")]
    public string loseTitle = "GAME OVER";
    public string winTitle = "YOU WIN!";
    public string menuSceneName = "Menu2";

    private ScoreManager scoreManager;
    public bool IsScreenVisible => loseScreen != null && loseScreen.activeSelf;

    private void Awake()
    {
        scoreManager = FindAnyObjectByType<ScoreManager>();
        CacheReferences();
        BindFallbackButtons();

        if (loseScreen != null)
        {
            loseScreen.SetActive(false);
        }
    }

    public void ShowLoseScreen()
    {
        ShowEndScreen(loseTitle, false);
    }

    public void ShowWinScreen()
    {
        ShowEndScreen(winTitle, true);
    }

    public void RestartGame()
    {
        ButtonScript.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMenu()
    {
        ButtonScript.LoadScene(menuSceneName);
    }

    private void ShowEndScreen(string title, bool didWin)
    {
        if (IsScreenVisible)
        {
            return;
        }

        PauseManager pauseManager = FindAnyObjectByType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.CloseForEndScreen();
        }

        Time.timeScale = 0f;
        if (loseScreen == null)
        {
            Debug.LogError("LoseScreenManager: Assign the end-screen panel.", this);
            return;
        }

        CacheReferences();
        loseScreen.SetActive(true);

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
        }

        if (scoreManager == null)
        {
            return;
        }

        int finalScore = scoreManager.GetScore();
        int highScore = scoreManager.GetHighScore();
        int targetScore = scoreManager.GetTargetScore();

        if (finalScoreText != null)
        {
            string scoreDisplay = targetScore > 0
                ? $"{finalScore:0000}/{targetScore:0000}"
                : finalScore.ToString("0000");
            finalScoreText.text = (didWin ? "Cleared: " : "Score: ") + scoreDisplay;
        }

        if (highScoreText != null)
        {
            highScoreText.text = "Best: " + highScore.ToString("0000");
        }
    }

    private void CacheReferences()
    {
        if (loseScreen == null)
        {
            return;
        }

        if (titleText == null)
        {
            titleText = FindTextByName("GameOver");
        }

        if (finalScoreText == null)
        {
            finalScoreText = FindTextByName("Score");
        }

        if (highScoreText == null)
        {
            highScoreText = FindTextByName("HighScore");
        }

        if (restartButton == null)
        {
            restartButton = FindButtonByName("RestartButton");
        }

        if (menuButton == null)
        {
            menuButton = FindButtonByName("MenuButton");
        }
    }

    private void BindFallbackButtons()
    {
        BindButtonIfMissing(restartButton, RestartGame);
        BindButtonIfMissing(menuButton, LoadMenu);
    }

    private void BindButtonIfMissing(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this &&
                button.onClick.GetPersistentMethodName(i) == action.Method.Name &&
                button.onClick.GetPersistentListenerState(i) != UnityEventCallState.Off)
            {
                return;
            }
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void OnDestroy()
    {
        if (restartButton != null) restartButton.onClick.RemoveListener(RestartGame);
        if (menuButton != null) menuButton.onClick.RemoveListener(LoadMenu);
    }

    private TextMeshProUGUI FindTextByName(string objectName)
    {
        foreach (var text in loseScreen.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.gameObject.name == objectName)
            {
                return text;
            }
        }

        return null;
    }

    private Button FindButtonByName(string objectName)
    {
        foreach (var button in loseScreen.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name == objectName)
            {
                return button;
            }
        }

        return null;
    }
}
