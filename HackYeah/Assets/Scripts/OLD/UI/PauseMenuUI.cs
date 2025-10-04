using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuPanel;

    private void OnEnable()
    {
        GameManager.OnGamePaused += TogglePauseMenu;
    }

    private void OnDisable()
    {
        GameManager.OnGamePaused -= TogglePauseMenu;
    }

    private void Start()
    {
        // Ensure the pause menu is hidden at the start of the game.
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    public void TogglePauseMenu(bool isPaused)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
            if(isPaused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    public void RestartScene()
    {
        Time.timeScale = 1f; // Przywr�� normalny czas gry
        ScoreManager.instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // This method will be called by the "Resume" button in the UI.
    public void ResumeGame()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGame();
        }
    }

    // This method will be called by the "Quit" button in the UI.
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        // If running in the Unity Editor, stop playing.
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
