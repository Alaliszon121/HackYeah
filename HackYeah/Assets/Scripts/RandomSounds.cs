using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Ustawienia d�wi�k�w")]
    public AudioClip[] sounds;          // Tablica z d�wi�kami
    public float interval = 20f;        // Czas mi�dzy odtwarzaniem (sekundy)
    
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

        // Co 20 sekund odtw�rz losowy d�wi�k
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
            Debug.LogWarning("Brak d�wi�k�w w tablicy!");
            return;
        }

        // Wybierz losowy indeks
        int index = Random.Range(0, sounds.Length);

        // Odtw�rz d�wi�k
        audioSource.clip = sounds[index];
        audioSource.Play();
    }
}

