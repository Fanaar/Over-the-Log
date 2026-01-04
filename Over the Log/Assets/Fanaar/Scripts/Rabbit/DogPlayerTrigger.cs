using UnityEngine;

public class DogPlayerTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;     // AudioSource voor de trigger
    public AudioClip triggerClip;       // Clip die afspeelt bij collision

    private bool hasPlayed = false;     // Check of audio al een keer is afgespeeld

    void OnTriggerEnter(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
        {
            dog.isInPlayerTrigger = true; // hond stil

            // Speel audio alleen als het nog niet is afgespeeld
            if (!hasPlayed && audioSource != null && triggerClip != null)
            {
                audioSource.PlayOneShot(triggerClip);
                hasPlayed = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
            dog.isInPlayerTrigger = false; // hond beweegt
    }
}
