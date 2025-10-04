using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a list of elements to create an infinite scrolling effect.
/// Elements can move up or down and will loop accordingly.
/// </summary>
public class MovingWalls : MonoBehaviour
{
    [Tooltip("The list of wall elements to scroll. All elements should be the same height.")]
    public List<Transform> scrollingElements;

    [Tooltip("The speed at which the elements will move.")]
    public float moveSpeed = 5f;

    private float elementHeight;
    private float totalHeight;
    private float topResetThreshold;
    private float bottomResetThreshold;
    private bool isMovingUp = true;

    private void OnEnable()
    {
        GameEvents.OnGravityInverted += HandleGravityInversion;
    }

    private void OnDisable()
    {
        GameEvents.OnGravityInverted -= HandleGravityInversion;
    }

    void Start()
    {
        if (scrollingElements == null || scrollingElements.Count == 0)
        {
            Debug.LogError("MovingWalls script requires at least 1 element in the 'scrollingElements' list.", this);
            enabled = false; 
            return;
        }

        // --- New, More Robust Initialization ---
        
        // Step 1: Get the height from the element's collider. Assumes all elements are the same height.
        Collider elementCollider = scrollingElements[0].GetComponentInChildren<Collider>();
        if (elementCollider == null)
        {
            Debug.LogError("The first element in 'scrollingElements' must have a Collider to measure its height.", scrollingElements[0]);
            enabled = false;
            return;
        }
        elementHeight = elementCollider.bounds.size.y;

        if (elementHeight <= 0.01f)
        {
            Debug.LogError("Element height is zero or negative. Cannot set up scrolling.", scrollingElements[0]);
            enabled = false;
            return;
        }
        
        // Step 2: Calculate the total height of the entire chain.
        totalHeight = elementHeight * scrollingElements.Count;

        // Step 3: Find the absolute highest and lowest points among all elements.
        float topmostY = float.NegativeInfinity;
        float bottommostY = float.PositiveInfinity;
        foreach (Transform element in scrollingElements)
        {
            if (element.position.y > topmostY) topmostY = element.position.y;
            if (element.position.y < bottommostY) bottommostY = element.position.y;
        }
        
        // Step 4: Calculate the thresholds for teleporting.
        topResetThreshold = topmostY + (elementHeight / 2);
        bottomResetThreshold = bottommostY - (elementHeight / 2);
    }

    void Update()
    {
        // Sync the wall speed with the platform speed from the spawner.
        if (spawning.instance != null)
        {
            moveSpeed = spawning.instance.realSpeed;
        }

        if (scrollingElements.Count == 0) return;

        Vector3 moveDirection = isMovingUp ? Vector3.up : Vector3.down;
        
        foreach (Transform element in scrollingElements)
        {
            element.position += moveDirection * moveSpeed * Time.deltaTime;

            if (isMovingUp)
            {
                // If element moves past the top, teleport it to the bottom.
                if (element.position.y > topResetThreshold)
                {
                    element.position -= Vector3.up * totalHeight;
                }
            }
            else
            {
                // If element moves past the bottom, teleport it to the top.
                if (element.position.y < bottomResetThreshold)
                {
                    element.position += Vector3.up * totalHeight;
                }
            }
        }
    }

    private void HandleGravityInversion(bool isReversed)
    {
        // When gravity is reversed (isReversed = true), platforms move DOWN.
        // We want the walls to move UP to create opposing motion.
        // Therefore, isMovingUp should be true when isReversed is true.
        isMovingUp = !isReversed;
    }
}
