using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public static event Action OnNewHighScore;

    private TMP_Text scoreText;
    private TMP_Text highScoreText;
    
    private float currentScore = 0f;
    private float highScore = 0f;

    private const string HighScoreKey = "HighScore";

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        highScore = PlayerPrefs.GetFloat(HighScoreKey, 0);
    }

    void Update()
    {
        UpdateScoreText();
        UpdateHighScoreText();
    }
    
    public void AssignUIElements(TMP_Text scoreText, TMP_Text highScoreText)
    {
        this.scoreText = scoreText;
        this.highScoreText = highScoreText;
    }

    public void AddScore(float amount)
    {
        if(currentScore + amount < 0)
        {
            currentScore = 0;
        }
        else
        {
            currentScore += amount;
        }

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetFloat(HighScoreKey, highScore);
            OnNewHighScore?.Invoke();
        }
    }

    public float GetCurrentScore()
    {
        return currentScore;
    }

    public void ResetScore()
    {
        currentScore = 0f;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            // Display score as an integer
            scoreText.text = "Score: " + Mathf.RoundToInt(currentScore).ToString();
        }
    }

    private void UpdateHighScoreText()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + Mathf.RoundToInt(highScore).ToString();
        }
    }
}
