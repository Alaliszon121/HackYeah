using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static event Action<bool> OnGamePaused;

    [Header("Scene Names")]
    public string loseSceneName = "LoseScreen"; // Create a scene with this name

    public bool IsGamePaused { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(true);
        UIManager.instance.pauseMenuPanel.SetActive(true);
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1f;
        OnGamePaused?.Invoke(false);
        UIManager.instance.pauseMenuPanel.SetActive(false);
        Debug.Log("Game Resumed");
    }

    public void GameOver()
    {
        if (IsGamePaused) return; // Prevent multiple game over triggers.

        IsGamePaused = true;
        Time.timeScale = 0f;
        GameEvents.TriggerGameOver();
        Debug.Log("Game Over!");
    }
}
