using UnityEngine;

public class DelayedAudioPlay : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + gameObject.name);
            return;
        }

        Invoke(nameof(PlayAudio), 10f);
    }

    private void PlayAudio()
    {
        audioSource.Play();
    }
}
