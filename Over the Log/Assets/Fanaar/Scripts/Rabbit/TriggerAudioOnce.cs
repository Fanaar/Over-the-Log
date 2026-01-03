using UnityEngine;

public class TriggerAudioOnce : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource; // Sleep hier je AudioSource in
    public AudioClip clip;          // De clip die moet afspelen

    private bool hasPlayed = false; // Zorgt dat het maar 1 keer speelt

    private void OnTriggerEnter(Collider other)
    {
        // Check dat het de speler is (optioneel)
        if (!hasPlayed && other.CompareTag("Player"))
        {
            audioSource.PlayOneShot(clip);
            hasPlayed = true;
        }
    }
}
