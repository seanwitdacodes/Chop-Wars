using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject scoreboardCanvas;   // drag your scoreboard canvas here
    public TextMeshProUGUI scoreText;     // drag your TMP text object here

    private int score = 0;
    private int highscore = 0;
    private float timer = 0f;

    void Start()
    {
        // Load saved highscore
        highscore = PlayerPrefs.GetInt("Highscore", 0);
        UpdateUI();
    }

    void Update()
    {
        // ⏱️ Time-based scoring
        timer += Time.deltaTime;

        if (timer >= 1f) // every 1 second
        {
            score++;
            timer -= 1f; // safer than reset (handles lag better)

            if (score > highscore)
            {
                highscore = score;
                PlayerPrefs.SetInt("Highscore", highscore);
                PlayerPrefs.Save();
            }

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString(); // ✅ just the number
    }

    public int GetScore()
    {
        return score;
    }

    public int GetHighScore()
    {
        return highscore;
    }

    public void HideScoreboard()
    {
        if (scoreboardCanvas != null)
            scoreboardCanvas.SetActive(false);
    }

    // 🪙 AddScore method for coins/hearts/etc.
    public void AddScore(int amount)
    {
        score += amount;

        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt("Highscore", highscore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }
}
