using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoseScreenUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loseScreenPanel;
    public TMP_Text finalScoreText;
    public TMP_Text highScoreText;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu"; // Change this to your main menu scene name

    private void OnEnable()
    {
        GameEvents.OnGameOver += ShowLoseScreen;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= ShowLoseScreen;
    }

    void Start()
    {
        // Ensure the lose screen is hidden at the start of the game.
        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(false);
        }
    }

    private void ShowLoseScreen()
    {
        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(true);
        }

        // Display the final score and high score.
        if (ScoreManager.instance != null)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = "Your Score: " + Mathf.RoundToInt(ScoreManager.instance.GetCurrentScore()).ToString();
            }

            if (highScoreText != null)
            {
                highScoreText.text = "High Score: " + Mathf.RoundToInt(PlayerPrefs.GetFloat("HighScore", 0)).ToString();
            }
        }
    }

    public void RestartGame()
    {
        // Unpause the game before reloading the scene.
        Time.timeScale = 1f;

        // Reset the score.
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.ResetScore();
        }

        // Reload the current scene to restart.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // Unpause the game before changing scenes.
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
