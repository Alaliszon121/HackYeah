using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static DspTimeSpawner;

public class FastInputMaterialSwapper : MonoBehaviour
{
    [Header("Input System")]
    public GameInputActions controls;

    [Header("Objects & Materials")]
    public Renderer[] targetRenderers;
    public Material swapMaterial;
    private Material[] originalMaterials;

    [Header("Swap Settings")]
    public float swapDuration = 0.12f;

    [Header("Judgement Windows (seconds)")]
    public float perfectWindow = 0.05f;
    public float goodWindow = 0.15f;

    [Header("Spawner Reference")]
    public DspTimeSpawner spawner; // assign in inspector (reads spawner.spawns)

    [Header("Particles")]
    public ParticleSystem hitParticle;
    public ParticleSystem missParticle;
    public ParticleSystem perfectParticle;

    [Header("Audio Feedback")]
    public AudioSource pitchShiftSource;
    public float missPitch = 0.5f;
    public float missPitchDuration = 0.5f;

    // Events for external scripts to hook into
    public event Action<TimedSpawn> OnMiss;
    public event Action<TimedSpawn> OnHit;

    // internals
    private double dspStartTime;
    private bool[] pressedThisFrame = new bool[4]; // lanes 0..3

    private void Awake()
    {
        controls = new GameInputActions();
        controls.Player.PlayA.performed += ctx => OnButtonPress(0);
        controls.Player.PlayB.performed += ctx => OnButtonPress(1);
        controls.Player.PlayC.performed += ctx => OnButtonPress(2);
        controls.Player.PlayD.performed += ctx => OnButtonPress(3);

        originalMaterials = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
            originalMaterials[i] = targetRenderers[i].material;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        dspStartTime = AudioSettings.dspTime;
    }

    private void Update()
    {
        double elapsed = AudioSettings.dspTime - dspStartTime;

        for (int lane = 0; lane < pressedThisFrame.Length; lane++)
        {
            if (!pressedThisFrame[lane]) continue;
            pressedThisFrame[lane] = false;

            if (lane < targetRenderers.Length)
                StartCoroutine(SwapMaterialTemporary(lane));

            HandleLaneInput(lane, elapsed);
        }

        AutoMissExpiredNotes(elapsed);
    }

    private void OnButtonPress(int lane)
    {
        if (lane < 0 || lane >= pressedThisFrame.Length) return;
        pressedThisFrame[lane] = true;
    }

    private void HandleLaneInput(int lane, double elapsed)
    {
        if (spawner == null)
        {
            Debug.LogWarning("FastInputMaterialSwapper: spawner reference is null.");
            return;
        }

        int bestIndex = -1;
        double bestAbsDelta = double.MaxValue;
        bool hasUpcomingNote = false;

        for (int i = 0; i < spawner.spawns.Count; i++)
        {
            var note = spawner.spawns[i];
            if (note.judged) continue;
            if (note.laneIndex != lane) continue;

            double delta = elapsed - note.spawnTime;
            double absDelta = Math.Abs(delta);

            // check if there’s a note within 1.5s ahead
            if (note.spawnTime >= elapsed && note.spawnTime <= elapsed + 0.8f)
                hasUpcomingNote = true;

            if (absDelta < bestAbsDelta)
            {
                bestAbsDelta = absDelta;
                bestIndex = i;
            }
        }

        // 🛑 NEW: if no note is close AND no upcoming note within 1.5s, ignore this input
        if (bestIndex == -1 && !hasUpcomingNote)
        {
            Debug.Log($"Ignored tap on lane {lane} at t={elapsed:F3}s (no notes within 1.5s)");
            return;
        }

        // If no candidate note found -> don't consume anything
        if (bestIndex == -1)
        {
            Debug.Log($"Tap on lane {lane} at t={elapsed:F3}s but no note close enough to judge.");
            return;
        }

        var closestNote = spawner.spawns[bestIndex];
        double deltaTime = elapsed - closestNote.spawnTime;
        double absDeltaTime = Math.Abs(deltaTime);

        if (absDeltaTime <= perfectWindow)
        {
            Debug.Log($"PERFECT on lane {lane} Δ={deltaTime:F3}s (t={elapsed:F3}s)");
            JudgeHitAtIndex(bestIndex, isPerfect: true);
            return;
        }

        if (absDeltaTime <= goodWindow)
        {
            Debug.Log($"GOOD on lane {lane} Δ={deltaTime:F3}s (t={elapsed:F3}s)");
            JudgeHitAtIndex(bestIndex, isPerfect: false);
            return;
        }

        Debug.Log($"MISS (off-timed) on lane {lane} Δ={deltaTime:F3}s (t={elapsed:F3}s)");
        JudgeMissAtIndex(bestIndex);
    }

    private void AutoMissExpiredNotes(double elapsed)
    {
        if (spawner == null) return;

        // iterate backwards so we can safely remove consumed spawns
        for (int i = spawner.spawns.Count - 1; i >= 0; i--)
        {
            var note = spawner.spawns[i];
            if (note.judged) continue;

            if (elapsed > note.spawnTime + goodWindow)
            {
                Debug.Log($"MISS (expired) on lane {note.laneIndex} Δ={(elapsed - note.spawnTime):F3}s (t={elapsed:F3}s)");
                JudgeMissAtIndex(i);
            }
        }
    }

    // new: judge by index so we can remove the spawn from the list immediately
    private void JudgeHitAtIndex(int index, bool isPerfect)
    {
        if (spawner == null) return;
        if (index < 0 || index >= spawner.spawns.Count) return;

        var note = spawner.spawns[index];
        if (note.judged) return; // guard

        note.judged = true;

        Vector3 pos = note.prefab != null ? note.instance.gameObject.transform.position : Vector3.zero;

        if (hitParticle != null)
            Instantiate(hitParticle, pos, Quaternion.identity).Play();

        if (isPerfect && perfectParticle != null)
            Instantiate(perfectParticle, pos, Quaternion.identity).Play();

        if (note.prefab != null)
            Destroy(note.instance.gameObject);

        OnHit?.Invoke(note);

        // remove consumed spawn so it won't be processed again
        spawner.spawns.RemoveAt(index);
    }

    private void JudgeMissAtIndex(int index)
    {
        if (spawner == null) return;
        if (index < 0 || index >= spawner.spawns.Count) return;

        var note = spawner.spawns[index];
        if (note.judged) return; // guard

        note.judged = true;

        Vector3 pos = note.prefab != null ? note.instance.gameObject.transform.position : Vector3.zero;

        if (missParticle != null)
            Instantiate(missParticle, pos, Quaternion.identity).Play();

        if (pitchShiftSource != null)
            StartCoroutine(TemporaryPitchShift());

        if (note.prefab != null)
            Destroy(note.instance.gameObject);

        OnMiss?.Invoke(note);

        // remove consumed spawn so it won't be processed again
        spawner.spawns.RemoveAt(index);
    }

    private IEnumerator TemporaryPitchShift()
    {
        float originalPitch = pitchShiftSource.pitch;
        pitchShiftSource.pitch = missPitch;
        yield return new WaitForSeconds(missPitchDuration);
        pitchShiftSource.pitch = originalPitch;
    }

    private IEnumerator SwapMaterialTemporary(int index)
    {
        if (index < 0 || index >= targetRenderers.Length) yield break;

        var rend = targetRenderers[index];
        var orig = originalMaterials.Length > index ? originalMaterials[index] : rend.material;

        rend.material = swapMaterial;
        yield return new WaitForSeconds(swapDuration);
        rend.material = orig;
    }
}
