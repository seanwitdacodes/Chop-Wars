using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public GameObject scoreboardCanvas;   // drag your scoreboard canvas here
    public TextMeshProUGUI scoreText;

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
        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            score++;

            if (score > highscore)
            {
                highscore = score;
                PlayerPrefs.SetInt("Highscore", highscore); // save
                PlayerPrefs.Save();
            }

            UpdateUI();
            timer = 0f;
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
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
}