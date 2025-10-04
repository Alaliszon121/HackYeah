using System.Collections;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public GameObject newHighScoreNotification;
    public float notificationDuration = 3f;
    public GameObject pauseMenuPanel;
    public GameObject loseScreenPanel;

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

    private void Start()
    {
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AssignUIElements(scoreText, highScoreText);
        }
    }

    private void OnEnable()
    {
        ScoreManager.OnNewHighScore += ShowNewHighScoreNotification;
    }

    private void OnDisable()
    {
        ScoreManager.OnNewHighScore -= ShowNewHighScoreNotification;
    }

    private void ShowNewHighScoreNotification()
    {
        if (newHighScoreNotification != null)
        {
            StartCoroutine(ShowNotificationCoroutine());
        }
    }

    private IEnumerator ShowNotificationCoroutine()
    {
        newHighScoreNotification.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        newHighScoreNotification.SetActive(false);
    }
}
