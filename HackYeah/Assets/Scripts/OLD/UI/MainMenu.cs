using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    private AudioSource uiAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private string volumeParameter = "MasterVolume";

    [Header("Scene Settings")]
    [SerializeField] private int gameSceneID = 0;

    private void Awake()
    {
        if (uiAudioSource == null)
        {
            uiAudioSource = GetComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
        }
    }
    public void PlayGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene(gameSceneID);
    }
    public void QuitGame()
    {
        PlayButtonSound();
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        masterMixer.SetFloat(volumeParameter, dB);
    }

    private void PlayButtonSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }
}
