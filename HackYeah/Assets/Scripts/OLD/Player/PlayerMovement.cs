using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    public GameObject centerPointObject;
    public Vector3 centerPoint = Vector3.zero;
    public float maxDistance = 13f;

    [Header("Tilting Settings")]
    public float maxTiltAngle = 20f;
    public float tiltSpeed = 5f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.1f;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Cooldown")]
    public List<GameObject> legs;
    public float oneLegCooldown = 10f;
    public float twoLegsCooldown = 5f;
    public Slider dashBar;

    private Collider playerCollider;
    private float dashCharge = 1f; // Represents 0 to 1 (0% to 100%)
    private bool isDashing = false;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        centerPoint = centerPointObject.transform.position;
    }

    void Update()
    {
        Vector3 movement = GetMovementInput();

        if (!isDashing)
        {
            HandleMovement(movement);
            HandleTilting();
            HandleDashInput(movement);
        }
        
        HandleDashCharge();
        UpdateDashBar();
    }

    Vector3 GetMovementInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);

        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        return movement;
    }

    void HandleMovement(Vector3 movement)
    {
        Vector3 nextPosition = transform.position + movement * speed * Time.deltaTime;
        if (Vector3.Distance(nextPosition, centerPoint) > maxDistance)
        {
            nextPosition = centerPoint + (nextPosition - centerPoint).normalized * maxDistance;
        }
        transform.position = nextPosition;
    }

    void HandleTilting()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float targetTilt = -horizontalInput * maxTiltAngle;
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, targetTilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    void HandleDashCharge()
    {
        if (dashCharge >= 1f) return;

        int legCount = GetActiveLegCount();
        float currentCooldown = 0f;

        if (legCount == 1)
        {
            currentCooldown = oneLegCooldown;
        }
        else if (legCount >= 2)
        {
            currentCooldown = twoLegsCooldown;
        }

        if (currentCooldown > 0)
        {
            dashCharge += Time.deltaTime / currentCooldown;
        }

        dashCharge = Mathf.Clamp01(dashCharge);
    }

    void HandleDashInput(Vector3 movement)
    {
        if (Input.GetKeyDown(dashKey) && dashCharge >= 1f && movement.magnitude > 0.1f)
        {
            StartCoroutine(DashCoroutine(movement));
        }
    }

    IEnumerator DashCoroutine(Vector3 movement)
    {
        isDashing = true;
        dashCharge = 0f;

        Vector3 dashDirection = movement.normalized;
        
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            Vector3 nextPosition = transform.position + dashDirection * dashSpeed * Time.deltaTime;
            if (Vector3.Distance(nextPosition, centerPoint) > maxDistance)
            {
                nextPosition = centerPoint + (nextPosition - centerPoint).normalized * maxDistance;
            }
            transform.position = nextPosition;
            yield return null;
        }

        isDashing = false;
    }

    int GetActiveLegCount()
    {
        int count = 0;
        foreach (var leg in legs)
        {
            if (leg != null && leg.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }

    void UpdateDashBar()
    {
        if (dashBar != null)
        {
            dashBar.value = dashCharge;
        }
    }

    public void SetColliderEnabled(bool isEnabled)
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = isEnabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If player is immortal, ignore all collisions.
        if (SkillsManager.instance != null && SkillsManager.instance.IsPlayerImmortal)
        {
            return;
        }
        //TO DO: ADD VALID LOGIC FOR GAME OVER
        // Check if the player has collided with a platform.
        if (collision.gameObject.CompareTag("Platform"))
        {
            // Trigger the game over event.
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
        }
    }
}
