using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Ustawienia dŸwiêków")]
    public AudioClip[] sounds;          // Tablica z dŸwiêkami
    public float interval = 20f;        // Czas miêdzy odtwarzaniem (sekundy)
    
    private AudioSource audioSource;    // Komponent AudioSource
    private float timer = 0f;

    void Start()
    {
        // Pobierz lub dodaj AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Odliczanie czasu
        timer += Time.deltaTime;

        // Co 20 sekund odtwórz losowy dŸwiêk
        if (timer >= interval)
        {
            PlayRandomSound();
            timer = 0f;
        }
    }

    void PlayRandomSound()
    {
        if (sounds.Length == 0)
        {
            Debug.LogWarning("Brak dŸwiêków w tablicy!");
            return;
        }

        // Wybierz losowy indeks
        int index = Random.Range(0, sounds.Length);

        // Odtwórz dŸwiêk
        audioSource.clip = sounds[index];
        audioSource.Play();
    }
}

