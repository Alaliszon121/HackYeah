using System.Collections;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    public static SkillsManager instance;
    
    public PlayerMovement playerMovement; // Assign in inspector
    public float skillDuration = 1f;

    public bool IsPlayerImmortal { get; private set; }

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ActivateSkill1();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActivateSkill2();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActivateSkill3();
        }
    }

    // Skill 1: Slow down platforms
    public void ActivateSkill1()
    {
        StartCoroutine(SlowDownCoroutine());
    }

    private IEnumerator SlowDownCoroutine()
    {
        float originalSpeed = spawning.instance.platformSpeed;
        spawning.instance.platformSpeed = originalSpeed * 0.2f;// 5 times slower
        yield return new WaitForSeconds(skillDuration);
        spawning.instance.platformSpeed = originalSpeed;
    }

    // Skill 2: Speed up platforms and grant immortality
    public void ActivateSkill2()
    {
        StartCoroutine(SpeedUpAndImmortalityCoroutine());
    }

    private IEnumerator SpeedUpAndImmortalityCoroutine()
    {
        float originalSpeed = spawning.instance.platformSpeed;
        spawning.instance.platformSpeed = originalSpeed * 5f;
        IsPlayerImmortal = true;
        if (playerMovement != null) playerMovement.SetColliderEnabled(false);
        Debug.Log("Player is now IMMORTAL.");
        yield return new WaitForSeconds(skillDuration);
        spawning.instance.platformSpeed = originalSpeed * 1.0f;
        IsPlayerImmortal = false;
        if (playerMovement != null) playerMovement.SetColliderEnabled(true);
        Debug.Log("Player is no longer immortal.");
    }

    // Skill 3: Destroy all platforms
    public void ActivateSkill3()
    {
        StartCoroutine(DestroyAllPlatformsCoroutine());
    }

    private IEnumerator DestroyAllPlatformsCoroutine()
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Damagable");
        foreach (GameObject platform in platforms)
        {
            Destroy(platform);
        }
        yield return new WaitForSeconds(1);
    }
}
