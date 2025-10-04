using UnityEngine;

public class Portal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger has the "Player" tag.
        if (other.CompareTag("Player"))
        {
            // Call the InvertGravity method on the spawner.
            if (spawning.instance != null)
            {
                spawning.instance.InvertGravity();
            }

            // To prevent the portal from being used multiple times, we destroy it.
            // You could also disable it or play an effect here.
            Destroy(gameObject);
        }
    }
}
