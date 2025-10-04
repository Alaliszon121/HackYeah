using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(VideoPlayer))]
public class CutsceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "SampleScene"; // Change to your main game scene

    private VideoPlayer videoPlayer;
    private bool isSceneLoading = false;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        // Subscribe to the loopPointReached event, which fires when the video is over.
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        // Check for any key press to skip the cutscene.
        if (Input.anyKeyDown)
        {
            LoadGameScene();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        // Ensure we only try to load the scene once.
        if (!isSceneLoading)
        {
            isSceneLoading = true;
            Debug.Log("Cutscene finished or skipped. Loading game scene...");
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
