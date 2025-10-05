using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HitAnimationManager : MonoBehaviour
{
    [Header("Game Objects with Animators")]
    public GameObject[] animators = new GameObject[4];

    [Header("Animation State")]
    public string animationName = "Fly"; // animation to play on hit

    [Header("Post Processing")]
    public Volume globalVolume;
    public float saturationStep = -20f;
    public float lerpSpeed = 1.5f; // How fast to fade saturation

    private int hitCount = 0;
    private ColorAdjustments colorAdjustments;
    private float targetSaturation = 0f;
    private bool lerping = false;

    private void Start()
    {
        if (globalVolume != null &&
            globalVolume.profile.TryGet(out colorAdjustments))
        {
            targetSaturation = colorAdjustments.saturation.value;
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found on Global Volume!");
        }
    }

    private void Update()
    {
        if (lerping && colorAdjustments != null)
        {
            float current = colorAdjustments.saturation.value;
            float newVal = Mathf.Lerp(current, targetSaturation, Time.deltaTime * lerpSpeed);
            colorAdjustments.saturation.value = newVal;

            if (Mathf.Abs(newVal - targetSaturation) < 0.1f)
            {
                colorAdjustments.saturation.value = targetSaturation;
                lerping = false;
            }
        }
    }

    /// <summary>
    /// Call this function whenever a hit/miss is registered
    /// </summary>
    public void RegisterHit()
    {
        hitCount++;

        // ↓ Decrease saturation by 20 each time (down to min -100)
        if (colorAdjustments != null)
        {
            targetSaturation = Mathf.Max(-100f, targetSaturation + saturationStep);
            lerping = true;
        }

        if (hitCount <= animators.Length)
        {
            GameObject currentObj = animators[hitCount - 1];
            if (currentObj != null)
            {
                Destroy(currentObj);
            }
        }
        else
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Over! 5th hit registered.");
        // Implement your game-over logic here (UI, scene reload, etc.)
    }
}
