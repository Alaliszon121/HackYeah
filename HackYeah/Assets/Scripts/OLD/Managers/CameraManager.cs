using UnityEngine;
using Cinemachine;

/// <summary>
/// Manages Cinemachine Virtual Cameras to transition between states when gravity is inverted.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [Tooltip("The virtual camera for the normal, upward gravity state.")]
    public CinemachineVirtualCamera normalGravityVCam;

    [Tooltip("The virtual camera for the reversed, downward gravity state.")]
    public CinemachineVirtualCamera reversedGravityVCam;

    [Header("Camera Priorities")]
    [Tooltip("The priority for the active virtual camera.")]
    public int activePriority = 20;

    [Tooltip("The priority for the inactive virtual camera.")]
    public int inactivePriority = 10;

    private void OnEnable()
    {
        GameEvents.OnGravityInverted += HandleGravityInversion;
    }

    private void OnDisable()
    {
        GameEvents.OnGravityInverted -= HandleGravityInversion;
    }

    private void Start()
    {
        // Set the initial camera state based on the spawner's starting gravity.
        if (spawning.instance != null)
        {
            HandleGravityInversion(spawning.instance.isGravityReversed);
        }
        else
        {
            // Default to normal gravity if the spawner isn't found.
            HandleGravityInversion(false);
        }
    }

    private void HandleGravityInversion(bool isReversed)
    {
        if (normalGravityVCam == null || reversedGravityVCam == null)
        {
            Debug.LogError("One or more virtual cameras are not assigned in the CameraManager.", this);
            return;
        }

        if (isReversed)
        {
            // Reversed gravity is active: give the reversed VCam higher priority.
            normalGravityVCam.Priority = inactivePriority;
            reversedGravityVCam.Priority = activePriority;
        }
        else
        {
            // Normal gravity is active: give the normal VCam higher priority.
            normalGravityVCam.Priority = activePriority;
            reversedGravityVCam.Priority = inactivePriority;
        }
    }
}
