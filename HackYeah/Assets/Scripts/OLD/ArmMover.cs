using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmMover : MonoBehaviour
{
    [Tooltip("A multiplier to adjust the arm's movement speed relative to the platform speed.")]
    public float speedMultiplier = 1.0f;
    public bool isRiped = false;

    void Update()
    {
        if (spawning.instance == null)
        {
            return;
        }

        float moveDirection = spawning.instance.isGravityReversed ? -1f : 1f;
        float moveAmount = spawning.instance.platformSpeed * speedMultiplier * Time.deltaTime;
        
        if (isRiped)
        {
            transform.position += new Vector3(0, moveDirection * moveAmount,0);
        }
    }
}
