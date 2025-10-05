using UnityEngine;

public class HitAnimationManager : MonoBehaviour
{
    [Header("Game Objects with Animators")]
    public GameObject[] animators = new GameObject[4];

    [Header("Animation State")]
    public string animationName = "Fly"; // animation to play on hit

    private int hitCount = 0;

    /// <summary>
    /// Call this function whenever a hit/miss is registered
    /// </summary>
    public void RegisterHit()
    {
        hitCount++;

        if (hitCount <= animators.Length)
        {
            // Play the hit animation on the correct game object
            GameObject currenObj = animators[hitCount - 1];
            if (currenObj != null)
            {
                Destroy(currenObj);
            }
        }
        else
        {
            // All 4 animations are done; handle game end
            EndGame();
        }
    }

    private void EndGame()
    {
        Debug.Log("Game Over! 5th hit registered.");
        // Implement your game-over logic here (UI, scene reload, etc.)
    }
}
