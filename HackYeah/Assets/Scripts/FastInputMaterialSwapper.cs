using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // internals
    private double dspStartTime;
    private bool[] pressedThisFrame = new bool[4]; // lanes 0..3

    private void Awake()
    {
        // prepare input actions (callbacks only set a flag; actual judgement happens in Update)
        controls = new GameInputActions();
        controls.Player.PlayA.performed += ctx => OnButtonPress(0);
        controls.Player.PlayB.performed += ctx => OnButtonPress(1);
        controls.Player.PlayC.performed += ctx => OnButtonPress(2);
        controls.Player.PlayD.performed += ctx => OnButtonPress(3);

        // store original materials (so we can restore)
        originalMaterials = new Material[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
            originalMaterials[i] = targetRenderers[i].material;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        // count time from the start of the scene (DSP)
        dspStartTime = AudioSettings.dspTime;
    }

    private void Update()
    {
        double elapsed = AudioSettings.dspTime - dspStartTime;

        // Process any input that arrived since last frame (we set flags in callbacks)
        for (int lane = 0; lane < pressedThisFrame.Length; lane++)
        {
            if (!pressedThisFrame[lane]) continue;
            pressedThisFrame[lane] = false; // consume

            // visual feedback
            if (lane < targetRenderers.Length)
                StartCoroutine(SwapMaterialTemporary(lane));

            // judge the press for this lane
            HandleLaneInput(lane, elapsed);
        }

        // Each frame: auto-miss any note that's expired (player did not hit it within goodWindow)
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

        // Find closest unjudged note in this lane (by absolute time difference)
        int bestIndex = -1;
        double bestAbsDelta = double.MaxValue;

        for (int i = 0; i < spawner.spawns.Count; i++)
        {
            var note = spawner.spawns[i];
            if (note.judged) continue;
            if (note.laneIndex != lane) continue;

            double delta = elapsed - note.spawnTime; // signed: negative = early, positive = late
            double absDelta = Math.Abs(delta);

            if (absDelta < bestAbsDelta)
            {
                bestAbsDelta = absDelta;
                bestIndex = i;
            }
        }

        // If no candidate note found -> immediate miss (no notes on this lane)
        if (bestIndex == -1)
        {
            Debug.Log($"MISS (no note) on lane {lane} at t={elapsed:F3}s");
            return;
        }

        // Judge the closest note
        var closestNote = spawner.spawns[bestIndex];
        double deltaTime = elapsed - closestNote.spawnTime;
        double absDeltaTime = Math.Abs(deltaTime);

        if (absDeltaTime <= perfectWindow)
        {
            Debug.Log($"PERFECT on lane {lane} Δ={deltaTime:F3}s (t={elapsed:F3}s)");
            closestNote.judged = true;
            spawner.spawns[bestIndex] = closestNote;
            return;
        }

        if (absDeltaTime <= goodWindow)
        {
            Debug.Log($"GOOD on lane {lane} Δ={deltaTime:F3}s (t={elapsed:F3}s)");
            closestNote.judged = true;
            spawner.spawns[bestIndex] = closestNote;
            return;
        }

        // NOT within windows -> declare miss (per your specification)
        // This consumes the closest note (marks it judged as missed).
        Debug.Log($"MISS (off-timed) on lane {lane} Δ={deltaTime:F3}s (t={elapsed:F3}s)");
        closestNote.judged = true;
        spawner.spawns[bestIndex] = closestNote;
    }

    private void AutoMissExpiredNotes(double elapsed)
    {
        if (spawner == null) return;

        for (int i = 0; i < spawner.spawns.Count; i++)
        {
            var note = spawner.spawns[i];
            if (note.judged) continue;

            // if current time passed the note's spawnTime + goodWindow -> auto-miss
            if (elapsed > note.spawnTime + goodWindow)
            {
                note.judged = true;
                spawner.spawns[i] = note;
                Debug.Log($"MISS (expired) on lane {note.laneIndex} Δ={(elapsed - note.spawnTime):F3}s (t={elapsed:F3}s)");
            }
        }
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
