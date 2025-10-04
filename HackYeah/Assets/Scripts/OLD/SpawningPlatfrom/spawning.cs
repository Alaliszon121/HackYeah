using System.Collections.Generic;
using UnityEngine;

public class spawning : MonoBehaviour
{
    public static spawning instance;

    [Header("Platform Prefabs")]
    public GameObject[] platformPrefabs;

    [Header("Ceiling Platform")]
    public GameObject ceilingPrefab;
    public float scoreThreshold = 10f;

    [Header("Special Object Spawning")]
    public GameObject talkingObjectPrefab;
    public int minSpawnInterval = 4;
    public int maxSpawnInterval = 5;

    [Header("Power-Up Spawning")]
    public GameObject[] powerUpPrefabs;
    public int minPowerUpInterval = 8;
    public int maxPowerUpInterval = 10;

    [Header("Spawning Configuration")]
    public float spawnX = 0f;
    // The vertical gap to leave between platforms.
    public float gapBetweenPlatforms = 5f;

    [Header("Vertical Boundaries")]
    public float topSpawnY = 20f;
    public float bottomSpawnY = -10f;
    public float topDestroyY = 25f;
    public float bottomDestroyY = -15f;
    public float spawnZ = -10f;
    public float ceillingZ = 0f;

    [Header("Movement")]
    public float platformSpeed = 10f;
    public AnimationCurve speedOverTime;
    // --- Private State ---
    private List<GameObject> activePlatforms = new List<GameObject>();
    private GameObject lastSpawnedPlatform;
    public bool isGravityReversed = false;
    private float elapsedTime = 0f;
    
    // Spawn counters for special items
    private int platformSpawnCount = 0;
    private int nextSpecialSpawnTarget;
    private int powerUpSpawnCount = 0;
    private int nextPowerUpSpawnTarget;

    public float realSpeed = 1f;
    // Flag to ensure the immediate ceiling spawn only happens once per entry into the danger zone.
    private bool dangerZoneCeilingSpawned = false;

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

    void Start()
    {
        SetNextSpecialSpawnTarget();
        SetNextPowerUpSpawnTarget();
        SpawnPlatform();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.G))
        {
            InvertGravity();
        }

        // Check for and handle the transition into the "danger zone"
        HandleDangerZone();

        HandlePlatformMovement();
        TrySpawnNewPlatform();
        CleanupOffscreenPlatforms();

        if(isGravityReversed)
        {
            ScoreManager.instance.AddScore(-1.0f);
        }
        else
        {
            ScoreManager.instance.AddScore(+1.0f);
        }
        realSpeed = (speedOverTime.Evaluate(elapsedTime/100)*platformSpeed)+10;
    }

    void HandleDangerZone()
    {
        bool inDangerZone = isGravityReversed && ceilingPrefab != null && ScoreManager.instance.GetCurrentScore() <= scoreThreshold;

        if (inDangerZone)
        {
            if (!dangerZoneCeilingSpawned)
            {
                // We've just entered the danger zone, force-spawn a ceiling immediately.
                ForceSpawnCeilingAfterLastPlatform();
                dangerZoneCeilingSpawned = true;
            }
        }
        else
        {
            // We are no longer in the danger zone, reset the flag.
            dangerZoneCeilingSpawned = false;
        }
    }

    void HandlePlatformMovement()
    {
        Vector3 moveDirection = isGravityReversed ? Vector3.down : Vector3.up;
        float moveAmount =  realSpeed * Time.deltaTime;

        for (int i = 0; i < activePlatforms.Count; i++)
        {
            if (activePlatforms[i] != null)
            {
                activePlatforms[i].transform.Translate(moveDirection * moveAmount, Space.World);
            }
        }
    }

    void TrySpawnNewPlatform()
    {
        bool shouldSpawn = false;
        if (lastSpawnedPlatform == null)
        {
            // If there's no reference platform, we must spawn one.
            shouldSpawn = true;
        }
        else
        {
            // Determine the distance the last platform has traveled from its spawn point.
            float spawnPointY = isGravityReversed ? topSpawnY : bottomSpawnY;
            float distanceTraveled = Mathf.Abs(lastSpawnedPlatform.transform.position.y - spawnPointY);

            // Get the size of the last platform to ensure the new one doesn't overlap.
            Collider lastPlatformCollider = lastSpawnedPlatform.GetComponentInChildren<Collider>();
            if (lastPlatformCollider != null)
            {
                float lastPlatformHeight = lastPlatformCollider.bounds.size.y;
                // We can spawn a new platform once the last one has moved its entire height plus the desired gap.
                if (distanceTraveled >= lastPlatformHeight + gapBetweenPlatforms)
                {
                    shouldSpawn = true;
                }
            }
            else
            {
                // Fallback for platforms without a collider. This is not ideal as it doesn't account for platform size.
                Debug.LogWarning("Last spawned platform is missing a Collider. Spacing will be based on gap alone.", lastSpawnedPlatform);
                if (distanceTraveled >= gapBetweenPlatforms)
                {
                    shouldSpawn = true;
                }
            }
        }

        if (shouldSpawn)
        {
            SpawnPlatform();
        }
    }
    
    /// <summary>
    /// Spawns a platform at the designated spawn edge (top or bottom).
    /// This is the standard spawn method called by the distance checker.
    /// </summary>
    void SpawnPlatform()
    {
        if (platformPrefabs.Length == 0) return;
        
        GameObject prefabToSpawn;

        // Check if we should spawn a ceiling or a normal platform.
        if (isGravityReversed && ceilingPrefab != null && ScoreManager.instance.GetCurrentScore() <= scoreThreshold)
        {
            prefabToSpawn = ceilingPrefab;
        }
        else
        {
            prefabToSpawn = platformPrefabs[Random.Range(0, platformPrefabs.Length)];
        }

        float spawnY = isGravityReversed ? topSpawnY : bottomSpawnY;
        Vector3 spawnPosition = new Vector3(0, spawnY, spawnZ);

        InstantiateAndManagePlatform(prefabToSpawn, spawnPosition);
    }

    /// <summary>
    /// Spawns a ceiling platform immediately after the last spawned platform.
    /// This is a special spawn triggered when the player's score drops too low in reversed gravity.
    /// </summary>
    void ForceSpawnCeilingAfterLastPlatform()
    {
        if (lastSpawnedPlatform == null || ceilingPrefab == null)
        {
            // If there's no reference platform, just do a normal spawn. It will correctly pick the ceiling prefab.
            SpawnPlatform();
            return;
        }

        Collider lastCollider = lastSpawnedPlatform.GetComponentInChildren<Collider>();
        Collider ceilingCollider = ceilingPrefab.GetComponentInChildren<Collider>();

        if (lastCollider == null || ceilingCollider == null)
        {
            Debug.LogError("Cannot force-spawn ceiling: Last platform or ceiling prefab is missing a collider.");
            SpawnPlatform(); // Fallback to a normal spawn.
            return;
        }

        // Calculate spawn position to be directly above the last platform, plus the gap.
        // "After" a downward-moving platform means above it.
        float lastPlatformTopEdge = lastSpawnedPlatform.transform.position.y + lastCollider.bounds.extents.y;
        //float ceilingHalfHeight = ceilingCollider.bounds.extents.y;
        float spawnY = lastPlatformTopEdge + gapBetweenPlatforms;

        Vector3 spawnPosition = new Vector3(0, spawnY, ceillingZ);

        InstantiateAndManagePlatform(ceilingPrefab, spawnPosition);
    }

    /// <summary>
    /// The core platform instantiation logic. Handles overlap checks, instantiation, and updating all lists and counters.
    /// </summary>
    void InstantiateAndManagePlatform(GameObject prefabToSpawn, Vector3 spawnPosition)
    {
        // --- Overlap Check ---
        Collider prefabCollider = prefabToSpawn.GetComponentInChildren<Collider>();
        if (prefabCollider == null) {
            Debug.LogError("Platform prefab is missing a Collider component! Cannot perform overlap check.", prefabToSpawn);
            return;
        }
        
        Vector3 checkHalfExtents = prefabCollider.bounds.extents;
        
        Collider[] hitColliders = Physics.OverlapBox(spawnPosition, checkHalfExtents, Quaternion.identity);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Platform"))
            {
                return; 
            }
        }
        // --- End Overlap Check ---

        GameObject newPlatform = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        activePlatforms.Add(newPlatform);
        lastSpawnedPlatform = newPlatform;

        // --- Handle Special Item Spawns ---
        platformSpawnCount++;
        if (platformSpawnCount >= nextSpecialSpawnTarget)
        {
            SpawnSpecialObjectOnPlatform(newPlatform);
            platformSpawnCount = 0;
            SetNextSpecialSpawnTarget();
        }

        powerUpSpawnCount++;
        if (powerUpSpawnCount >= nextPowerUpSpawnTarget)
        {
            SpawnPowerUpOnPlatform(newPlatform);
            powerUpSpawnCount = 0;
            SetNextPowerUpSpawnTarget();
        }

        if (!newPlatform.CompareTag("Platform"))
        {
            Debug.LogWarning($"Spawned platform '{newPlatform.name}' does not have the 'Platform' tag. The spawn overlap check may not work correctly.", newPlatform);
        }
    }

    void SetNextSpecialSpawnTarget()
    {
        nextSpecialSpawnTarget = Random.Range(minSpawnInterval, maxSpawnInterval + 1);
    }

    void SpawnSpecialObjectOnPlatform(GameObject platform)
    {
        if (talkingObjectPrefab == null)
        {
            Debug.LogWarning("Special Object Prefab is not assigned in the spawner.");
            return;
        }

        Platform platformComponent = platform.GetComponent<Platform>();
        if (platformComponent == null || platformComponent.specialSpawnPoints.Count == 0)
        {
            Debug.LogWarning($"Platform '{platform.name}' is missing the Platform component or has no special spawn points assigned.", platform);
            return;
        }

        // Pick a random spawn point from the list
        int spawnPointIndex = Random.Range(0, platformComponent.specialSpawnPoints.Count);
        Transform spawnPoint = platformComponent.specialSpawnPoints[spawnPointIndex];

        // Instantiate the special object and parent it to the spawn point
        Instantiate(talkingObjectPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
    }

    void SetNextPowerUpSpawnTarget()
    {
        nextPowerUpSpawnTarget = Random.Range(minPowerUpInterval, maxPowerUpInterval + 1);
    }

    void SpawnPowerUpOnPlatform(GameObject platform)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            Debug.LogWarning("Power-Up Prefabs are not assigned in the spawner.");
            return;
        }

        Platform platformComponent = platform.GetComponent<Platform>();
        if (platformComponent == null || platformComponent.powerUpSpawnPoints.Count == 0)
        {
            Debug.LogWarning($"Platform '{platform.name}' is missing the Platform component or has no power-up spawn points assigned.", platform);
            return;
        }
        
        // Pick a random power-up and a random spawn point
        GameObject powerUpToSpawn = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        Transform spawnPoint = platformComponent.powerUpSpawnPoints[Random.Range(0, platformComponent.powerUpSpawnPoints.Count)];

        // Instantiate the power-up and parent it to the spawn point
        Instantiate(powerUpToSpawn, spawnPoint.position, spawnPoint.rotation, spawnPoint);
    }

    void CleanupOffscreenPlatforms()
    {
        // Iterate backwards to safely remove items from the list.
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            GameObject platform = activePlatforms[i];
            if (platform == null)
            {
                activePlatforms.RemoveAt(i);
                continue;
            }

            bool shouldDestroy = false;
            if (isGravityReversed) // Moving down
            {
                if (platform.transform.position.y < bottomDestroyY) shouldDestroy = true;
            }
            else // Moving up
            {
                if (platform.transform.position.y > topDestroyY) shouldDestroy = true;
            }

            if (shouldDestroy)
            {
                if (platform == lastSpawnedPlatform)
                {
                    lastSpawnedPlatform = null; 
                }
                Destroy(platform);
                activePlatforms.RemoveAt(i);
            }
        }
    }

    public void InvertGravity()
    {
        isGravityReversed = !isGravityReversed;
        
        // Announce the gravity change to other systems (like the camera).
        GameEvents.TriggerGravityInverted(isGravityReversed);

        // Find the new "leading" platform to base our spawning on.
        // This is the platform closest to the *new* spawn point.
        lastSpawnedPlatform = FindClosestPlatformToSpawnPoint();
    }

    private GameObject FindClosestPlatformToSpawnPoint()
    {
        GameObject closest = null;
        float minDistance = float.MaxValue;
        float spawnPointY = isGravityReversed ? topSpawnY : bottomSpawnY;

        foreach (var platform in activePlatforms)
        {
            if (platform == null) continue;
            
            float distance = Mathf.Abs(platform.transform.position.y - spawnPointY);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = platform;
            }
        }
        return closest;
    }
}
