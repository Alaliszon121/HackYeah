using UnityEngine;

public class RandomAnimationOffset : MonoBehaviour
{
    public Animator animator;
    public string animationName;
    public float minOffset = 0f; // x seconds
    public float maxOffset = 1f; // y seconds

    void Start()
    {
        if (animator != null)
        {
            // Random offset in seconds
            float randomOffset = Random.Range(minOffset, maxOffset);
            animator.Play(animationName, 0, randomOffset / animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }
}
