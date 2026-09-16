using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject scoreboardCanvas;
    public TextMeshProUGUI scoreText;

    [Header("Endless Run")]
    [Tooltip("Keep this at zero for an endless run. A positive value is supported for legacy scenes only.")]
    public int targetScore = 0;
    public bool stopScoringWhenTargetReached = false;
    public float stageLength = 30f;
    public int healthyPickupPoints = 5;
    public int maxMultiplier = 5;

    [Header("Score Feedback")]
    public float bonusPulseScale = 1.18f;
    public float pulseDuration = 0.16f;
    public Color normalScoreColor = Color.white;
    public Color bonusScoreColor = new Color(1f, 0.92f, 0.45f, 1f);

    private const string HighscoreKey = "Highscore";
    private int score = 0;
    private int highscore = 0;
    private float timer = 0f;
    private float runTime;
    private int combo;
    private bool targetReached = false;
    private bool scoringStopped;
    private bool highScoreDirty;
    private Vector3 scoreTextBaseScale = Vector3.one;
    private Coroutine pulseRoutine;

    public event Action<int, int> TargetScoreReached;

    public bool IsScoringStopped => scoringStopped;
    public int Combo => combo;
    public int Multiplier => Mathf.Clamp(1 + combo / 4, 1, Mathf.Max(1, maxMultiplier));
    public int Stage => 1 + Mathf.FloorToInt(runTime / Mathf.Max(5f, stageLength));
    public float RunTime => runTime;

    private void Awake()
    {
        highscore = Mathf.Max(0, PlayerPrefs.GetInt(HighscoreKey, 0));
        ShowScoreboard();

        if (scoreText != null)
        {
            scoreTextBaseScale = scoreText.rectTransform.localScale;
            scoreText.color = normalScoreColor;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (scoringStopped || (targetReached && stopScoringWhenTargetReached))
        {
            return;
        }

        timer += Time.deltaTime;
        runTime += Time.deltaTime;

        while (timer >= 1f)
        {
            timer -= 1f;
            AddScoreInternal(1);

            if (scoringStopped || (targetReached && stopScoringWhenTargetReached))
            {
                break;
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE  {score:0000}";
        }
    }

    public int GetScore()
    {
        return score;
    }

    public int GetHighScore()
    {
        return highscore;
    }

    public int GetTargetScore()
    {
        return targetScore;
    }

    public void HideScoreboard()
    {
        if (scoreboardCanvas != null)
            scoreboardCanvas.SetActive(false);
    }

    public void ShowScoreboard()
    {
        if (scoreboardCanvas != null)
            scoreboardCanvas.SetActive(true);
    }

    public void AddScore(int amount)
    {
        if (amount == 0 || scoringStopped || (targetReached && stopScoringWhenTargetReached))
        {
            return;
        }

        AddScoreInternal(amount);
    }

    public void ResetScore()
    {
        SaveHighScore();
        ResetPulse();
        score = 0;
        timer = 0f;
        runTime = 0f;
        combo = 0;
        targetReached = false;
        scoringStopped = false;
        ShowScoreboard();

        if (scoreText != null)
        {
            scoreText.rectTransform.localScale = scoreTextBaseScale;
            scoreText.color = normalScoreColor;
        }

        UpdateUI();
    }

    public void StopScoring()
    {
        scoringStopped = true;
        SaveHighScore();
    }

    public void RegisterHealthyPickup()
    {
        if (scoringStopped)
        {
            return;
        }

        combo = Mathf.Min(combo + 1, 999);
        AddScoreInternal(Mathf.Max(0, healthyPickupPoints) * Multiplier);
    }

    public void RegisterCoin(int baseValue)
    {
        if (scoringStopped)
        {
            return;
        }

        combo = Mathf.Min(combo + 2, 999);
        AddScoreInternal(Mathf.Max(0, baseValue) * Multiplier);
    }

    public void BreakCombo()
    {
        combo = 0;
    }

    private void AddScoreInternal(int amount)
    {
        if (scoringStopped || (targetReached && stopScoringWhenTargetReached))
        {
            return;
        }

        score = (int)Math.Max(0L, Math.Min(int.MaxValue, (long)score + amount));

        if (score > highscore)
        {
            highscore = score;
            PlayerPrefs.SetInt(HighscoreKey, highscore);
            highScoreDirty = true;
        }

        UpdateUI();

        if (amount > 1 || score % 25 == 0)
        {
            PlayPulse(amount > 1 ? bonusScoreColor : normalScoreColor);
        }

        if (!targetReached && targetScore > 0 && score >= targetScore)
        {
            targetReached = true;
            TargetScoreReached?.Invoke(score, highscore);
        }
    }

    private void SaveHighScore()
    {
        if (!highScoreDirty)
        {
            return;
        }

        PlayerPrefs.Save();
        highScoreDirty = false;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveHighScore();
        }
    }

    private void OnApplicationQuit()
    {
        SaveHighScore();
    }

    private void OnDisable()
    {
        SaveHighScore();
        ResetPulse();
    }

    private void ResetPulse()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }

        if (scoreText != null)
        {
            scoreText.rectTransform.localScale = scoreTextBaseScale;
            scoreText.color = normalScoreColor;
        }
    }

    private void PlayPulse(Color pulseColor)
    {
        if (scoreText == null)
        {
            return;
        }

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulseRoutine = StartCoroutine(PulseScoreRoutine(pulseColor));
    }

    private System.Collections.IEnumerator PulseScoreRoutine(Color pulseColor)
    {
        float elapsed = 0f;
        float halfDuration = Mathf.Max(0.0001f, pulseDuration * 0.5f);
        Vector3 boostedScale = scoreTextBaseScale * bonusPulseScale;

        while (elapsed < pulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed < halfDuration
                ? elapsed / halfDuration
                : 1f - ((elapsed - halfDuration) / halfDuration);

            scoreText.rectTransform.localScale = Vector3.Lerp(scoreTextBaseScale, boostedScale, Mathf.Clamp01(t));
            scoreText.color = Color.Lerp(normalScoreColor, pulseColor, Mathf.Clamp01(t));
            yield return null;
        }

        scoreText.rectTransform.localScale = scoreTextBaseScale;
        scoreText.color = normalScoreColor;
        pulseRoutine = null;
    }
}
