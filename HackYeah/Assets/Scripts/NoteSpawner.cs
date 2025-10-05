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

    private Dictionary<string, AudioClip> soundMap;
    private List<(CinemachineDollyCart cart, NoteDef def)> activeNotes = new();

    // One hittable note per line
    private Dictionary<int, (CinemachineDollyCart cart, NoteDef def)?> currentHittable = new();

    private double songStartDspTime;

    [Header("Hit Windows (seconds)")]
    public float perfectWindow = 0.05f;
    public float goodWindow = 0.15f;
    public float missWindow = 0.3f;

    public HitAnimationManager hitAnimationManager;
    void Start()
    {
        // Prepare sound dictionary
        soundMap = new Dictionary<string, AudioClip>();
        foreach (var s in sounds)
            soundMap[s.ID] = s.audioClip;

        // Precompute spawn times
        double beatDuration = 60.0 / BPM;
        double currentTime = startOffset;

        for (int i = 0; i < notes.Length; i++)
        {
            double multiplier = NoteTypeToBeats(notes[i].noteType);
            notes[i].spawnTime = currentTime;
            currentTime += beatDuration * multiplier;
        }

        // Spawn all notes immediately
        for (int i = 0; i < notes.Length; i++)
        {
            if (notes[i].prefab != null && notes[i].path != null)
            {
                var clone = Instantiate(notes[i].prefab, Vector3.zero, Quaternion.identity);
                clone.m_Path = notes[i].path;
                clone.m_Position = (float)notes[i].spawnTime / 2f;

                activeNotes.Add((clone, notes[i]));
            }
        }

        // Init per-line state
        foreach (var note in notes)
            currentHittable[note.lineIndex] = null;

        // Start song
        songStartDspTime = AudioSettings.dspTime;
        musicSource.PlayScheduled(songStartDspTime);
    }

    void Update()
    {
        double songTime = AudioSettings.dspTime - songStartDspTime;

        // Update hittable notes per line
        foreach (var lineIndex in currentHittable.Keys)
        {
            // If no current hittable note, try to find one
            if (currentHittable[lineIndex] == null)
            {
                foreach (var (cart, def) in activeNotes)
                {
                    if (def.lineIndex != lineIndex) continue;

                    double delta = def.spawnTime - songTime;
                    if (Mathf.Abs((float)delta) <= missWindow)
                    {
                        currentHittable[lineIndex] = (cart, def);
                        Debug.Log($"[Line {lineIndex}] Note became hittable at time {songTime:F3}");
                        break;
                    }
                }
            }

            // If there is a hittable note, log it while active
            if (currentHittable[lineIndex] != null)
            {
                var (cart, def) = currentHittable[lineIndex].Value;
                double delta = songTime - def.spawnTime;

                if (Mathf.Abs((float)delta) <= missWindow)
                {
                    Debug.Log($"[Line {lineIndex}] Hittable note active (Δ={delta:F3}) at frame {Time.frameCount}");
                }
                else if (delta > missWindow)
                {
                    // Missed
                    hitAnimationManager.RegisterHit();
                    Debug.Log($"[Line {lineIndex}] Miss (note expired at time {songTime:F3})");
                    Destroy(cart.gameObject);
                    activeNotes.RemoveAll(n => n.cart == cart);
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

        if (currentHittable[lineIndex] != null)
        {
            var (cart, def) = currentHittable[lineIndex].Value;
            double delta = Mathf.Abs((float)(def.spawnTime - songTime));

            if (delta <= perfectWindow)
                Debug.Log($"[Line {lineIndex}] Perfect hit!");
            else if (delta <= goodWindow)
                Debug.Log($"[Line {lineIndex}] Good hit!");
            else { 
                Debug.Log($"[Line {lineIndex}] Late/Early hit (Δ={delta:F3})");
                
            }

            // Play sound if hit in good/perfect window
            if (delta <= goodWindow && soundMap.TryGetValue(def.soundID, out var clip))
                AudioSource.PlayClipAtPoint(clip, Vector3.zero);

            // Remove note
            Destroy(cart.gameObject);
            activeNotes.RemoveAll(n => n.cart == cart);
            currentHittable[lineIndex] = null;
        }
        else
        {
            // IGNORE input when no note is hittable → no miss
            Debug.Log($"[Line {lineIndex}] Input ignored (no note in window)");
        }
    }

}
