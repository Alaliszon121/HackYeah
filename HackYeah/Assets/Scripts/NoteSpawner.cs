using UnityEngine;
using System.Collections.Generic;
using Cinemachine;

public enum NoteType { Whole, Half, Quarter, Eighth, Sixteenth }

[System.Serializable]
public struct SoundDef
{
    public string ID;
    public AudioClip audioClip;
}

[System.Serializable]
public struct NoteDef
{
    public NoteType noteType;
    public string soundID;
    [HideInInspector] public double spawnTime;
    public CinemachineDollyCart prefab;
    public CinemachineSmoothPath path;
    public int lineIndex;
}

[System.Serializable]
public struct PathDef
{
    public int ID;
    public CinemachineSmoothPath path;
}

public class NoteSpawner : MonoBehaviour
{
    [Header("Song Settings")]
    public AudioSource musicSource;
    public float BPM = 120f;
    public float startOffset = 1f;

    [Header("Sounds")]
    public SoundDef[] sounds;

    [Header("Notes")]
    public NoteDef[] notes;

    [Header("Paths (IDs auto-assigned)")]
    public PathDef[] paths;

    [Header("Particles")]
    public ParticleSystem missParticle;
    public ParticleSystem hitParticle;
    public ParticleSystem perfectParticle;

    // how close to exact dsp time we require for scheduling fallback (tiny epsilon)
    private const double dspEpsilon = 0.0005;

    private Dictionary<string, AudioClip> soundMap;

    // internal representation of a spawned note
    private class NoteInstance
    {
        public CinemachineDollyCart cart;
        public NoteDef def;
        public bool consumed;         // was hit (good/perfect) or miss-handled
        public bool soundScheduled;   // if we already scheduled or played its sound
    }

    private List<NoteInstance> activeNotes = new();

    // One hittable note per line
    private Dictionary<int, NoteInstance> currentHittable = new();

    private double songStartDspTime;

    [Header("Hit Windows (seconds)")]
    public float perfectWindow = 0.05f;
    public float goodWindow = 0.15f;
    public float missWindow = 0.3f;

    public HitAnimationManager hitAnimationManager;

    void Start()
    {
        soundMap = new Dictionary<string, AudioClip>();
        foreach (var s in sounds)
            if (!string.IsNullOrEmpty(s.ID) && s.audioClip != null)
                soundMap[s.ID] = s.audioClip;

        // Compute spawn times
        double beatDuration = 60.0 / BPM;
        double currentTime = startOffset;
        for (int i = 0; i < notes.Length; i++)
        {
            notes[i].path = paths[notes[i].lineIndex].path;
            double multiplier = NoteTypeToBeats(notes[i].noteType);
            notes[i].spawnTime = currentTime;
            currentTime += beatDuration * multiplier;
        }

        // Spawn note carts and create NoteInstance for each note
        for (int i = 0; i < notes.Length; i++)
        {
            var n = notes[i];
            if (n.prefab != null && n.path != null)
            {
                var clone = Instantiate(n.prefab, Vector3.zero, Quaternion.identity);
                clone.m_Path = n.path;
                clone.m_Position = (float)n.spawnTime / 2f;

                var ni = new NoteInstance()
                {
                    cart = clone,
                    def = n,
                    consumed = false,
                    soundScheduled = false
                };
                activeNotes.Add(ni);
            }
        }

        // Init per-line hittable dictionary
        foreach (var n in notes)
            if (!currentHittable.ContainsKey(n.lineIndex))
                currentHittable[n.lineIndex] = null;

        // Start music
        songStartDspTime = AudioSettings.dspTime;
        musicSource.PlayScheduled(songStartDspTime);
    }

    void Update()
    {
        double songTime = AudioSettings.dspTime - songStartDspTime;

        // Update hittable notes
        List<int> keys = new List<int>(currentHittable.Keys);
        foreach (var lineIndex in keys)
        {
            if (currentHittable[lineIndex] == null)
            {
                foreach (var ni in activeNotes)
                {
                    if (ni.def.lineIndex != lineIndex) continue;
                    if (ni.consumed) continue;
                    double delta = ni.def.spawnTime - songTime;
                    if (Mathf.Abs((float)delta) <= missWindow)
                    {
                        currentHittable[lineIndex] = ni;
                        Debug.Log($"[Line {lineIndex}] Note became hittable at time {songTime:F3}");
                        break;
                    }
                }
            }

            if (currentHittable[lineIndex] != null)
            {
                var ni = currentHittable[lineIndex];
                double delta = songTime - ni.def.spawnTime;

                if (delta > missWindow)
                {
                    // Miss
                    ni.consumed = true;
                    Debug.Log($"[Line {lineIndex}] Miss (note expired at time {songTime:F3})");
                    hitAnimationManager?.RegisterHit();

                    SpawnParticle(missParticle, ni.cart.transform.position);

                    if (ni.cart != null) Destroy(ni.cart.gameObject);
                    activeNotes.Remove(ni);
                    currentHittable[lineIndex] = null;
                }
            }
        }
    }

    double NoteTypeToBeats(NoteType type)
    {
        return type switch
        {
            NoteType.Whole => 4,
            NoteType.Half => 2,
            NoteType.Quarter => 1,
            NoteType.Eighth => 0.5,
            NoteType.Sixteenth => 0.25,
            _ => 1
        };
    }

    public void TryHit(int lineIndex)
    {
        double songTime = AudioSettings.dspTime - songStartDspTime;

        if (!currentHittable.ContainsKey(lineIndex) || currentHittable[lineIndex] == null)
        {
            Debug.Log($"[Line {lineIndex}] Input ignored (no note in window)");
            return;
        }

        var ni = currentHittable[lineIndex];
        double deltaAbs = System.Math.Abs(ni.def.spawnTime - songTime);

        bool shouldPlaySound = false;

        if (deltaAbs <= perfectWindow)
        {
            Debug.Log($"[Line {lineIndex}] Perfect hit!");
            shouldPlaySound = true;
            SpawnParticle(perfectParticle, ni.cart.transform.position);
            SpawnParticle(hitParticle, ni.cart.transform.position); // also play hit
        }
        else if (deltaAbs <= goodWindow)
        {
            Debug.Log($"[Line {lineIndex}] Good hit!");
            shouldPlaySound = true;
            SpawnParticle(hitParticle, ni.cart.transform.position);
        }
        else
        {
            Debug.Log($"[Line {lineIndex}] Late/Early hit (Δ={deltaAbs:F3})");
            // no particles on bad hit
        }

        if (shouldPlaySound && !ni.soundScheduled)
        {
            if (soundMap.TryGetValue(ni.def.soundID, out var clip) && clip != null)
            {
                double scheduledDspTime = songStartDspTime + ni.def.spawnTime;
                if (scheduledDspTime > AudioSettings.dspTime + dspEpsilon)
                {
                    GameObject audioGO = new GameObject($"NoteAudio_{ni.def.soundID}_{ni.def.spawnTime:F3}");
                    audioGO.transform.parent = this.transform;
                    var a = audioGO.AddComponent<AudioSource>();
                    a.clip = clip;
                    a.playOnAwake = false;
                    a.spatialBlend = 0f;
                    a.loop = false;
                    a.PlayScheduled(scheduledDspTime);
                    Destroy(audioGO, (float)clip.length + 1.0f);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, ni.cart.transform.position);
                }
                ni.soundScheduled = true;
            }
        }

        ni.consumed = true;
        if (ni.cart != null) Destroy(ni.cart.gameObject);

        currentHittable[lineIndex] = null;
        activeNotes.Remove(ni);
    }

    private void SpawnParticle(ParticleSystem prefab, Vector3 pos)
    {
        if (prefab == null) return;
        var ps = Instantiate(prefab, pos, Quaternion.identity);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}
