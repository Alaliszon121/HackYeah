using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class HoldActionSlider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup sliderCanvasGroup;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Settings")]
    [SerializeField] private InputActionReference holdAction;
    [SerializeField] private float fillSpeed = 1f;
    [SerializeField] private float drainSpeed = 2f;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private int targetSceneBuildIndex = 1;

    private bool isHolding = false;
    private bool isActive = false;
    private float targetAlpha = 0f;
    private bool sceneLoaded = false;

    private void OnEnable()
    {
        if (holdAction != null)
        {
            holdAction.action.started += OnHoldStarted;
            holdAction.action.canceled += OnHoldCanceled;
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnded;
        }

        slider.value = 0f;
        sliderCanvasGroup.alpha = 0f;
        sliderCanvasGroup.interactable = false;
        sliderCanvasGroup.blocksRaycasts = false;
    }

    private void OnDisable()
    {
        if (holdAction != null)
        {
            holdAction.action.started -= OnHoldStarted;
            holdAction.action.canceled -= OnHoldCanceled;
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnded;
        }
    }

    private void Update()
    {
        if (isActive)
        {
            if (isHolding)
            {
                slider.value += fillSpeed * Time.deltaTime;

                if (slider.value >= 1f)
                {
                    slider.value = 1f;
                    OnSliderFilled();
                    ResetSlider();
                }
            }
            else
            {
                slider.value -= drainSpeed * Time.deltaTime;

                if (slider.value <= 0f)
                {
                    slider.value = 0f;
                    isActive = false;
                    targetAlpha = 0f;
                }
            }
        }

        sliderCanvasGroup.alpha = Mathf.MoveTowards(
            sliderCanvasGroup.alpha,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );
    }

    private void OnHoldStarted(InputAction.CallbackContext ctx)
    {
        isHolding = true;
        isActive = true;
        targetAlpha = 1f;
        sliderCanvasGroup.interactable = true;
        sliderCanvasGroup.blocksRaycasts = true;
    }

    private void OnHoldCanceled(InputAction.CallbackContext ctx)
    {
        isHolding = false;
    }

    private void OnSliderFilled()
    {
        LoadTargetScene();
    }

    private void ResetSlider()
    {
        isHolding = false;
        slider.value = 0f;
        isActive = false;
        targetAlpha = 0f;
        sliderCanvasGroup.interactable = false;
        sliderCanvasGroup.blocksRaycasts = false;
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        LoadTargetScene();
    }

    private void LoadTargetScene()
    {
        if (!sceneLoaded)
        {
            sceneLoaded = true;
            SceneManager.LoadScene(targetSceneBuildIndex);
        }
    }
}
