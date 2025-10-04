using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "SampleScene"; // Change to your main game scene
    public string cutsceneSceneName = "Cutscene"; // Change to your cutscene scene

    private const string HasPlayedBeforeKey = "HasPlayedBefore";

    public void StartGame()
    {
        // Check if the player has played before.
        if (PlayerPrefs.GetInt(HasPlayedBeforeKey, 0) == 0)
        {
            // First time playing: set the flag and go to the cutscene.
            PlayerPrefs.SetInt(HasPlayedBeforeKey, 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene(cutsceneSceneName);
        }
        else
        {
            // Not the first time: go straight to the game.
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
