using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoseScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loseScreen;          // Drag your LoseScreen panel here
    public TextMeshProUGUI finalScoreText; // Drag FinalScore TMP here
    public TextMeshProUGUI highScoreText;  // Drag HighScore TMP here

    private ScoreManager scoreManager;
   

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();

        // Hide lose screen at start
        if (loseScreen != null)
            loseScreen.SetActive(false);
    }

    public void ShowLoseScreen()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
            Time.timeScale = 0f; // pause the game

            if (scoreManager != null)
            {
                int finalScore = scoreManager.GetScore();
                int highScore = scoreManager.GetHighScore();

                if (finalScoreText != null)
                    finalScoreText.text = "Score: " + finalScore;

                if (highScoreText != null)
                    highScoreText.text = "Highscore: " + highScore;
            }
        }
    }

    // 🔄 Restart Button
    public void RestartGame()
    {
        Time.timeScale = 1f; // unpause game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    
    

}

