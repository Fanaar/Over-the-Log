using UnityEngine;

public class DogPlayerTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip triggerClip;

    private bool hasPlayed = false;

    void OnTriggerEnter(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
        {
            dog.isInPlayerTrigger = true;

            // Speel audio
            if (!hasPlayed && audioSource != null && triggerClip != null)
            {
                audioSource.PlayOneShot(triggerClip);
                hasPlayed = true;
            }

            if (StressManager.Instance != null)
            {
                StressManager.Instance.inDogTrigger = true;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null && StressManager.Instance != null)
        {
            StressManager.Instance.playerIsLookingAtDog = true; // blijf continu true zolang in trigger
        }
    }

    void OnTriggerExit(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
        {
            dog.isInPlayerTrigger = false;

            if (StressManager.Instance != null)
            {
                StressManager.Instance.inDogTrigger = false;
                StressManager.Instance.playerIsLookingAtDog = false; // reset
            }
        }
    }
}
