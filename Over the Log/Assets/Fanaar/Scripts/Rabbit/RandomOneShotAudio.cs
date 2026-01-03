using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomOneShotAudioOnEnable : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip[] audioClips;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        PlayRandomClip();
    }

    void PlayRandomClip()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned!", this);
            return;
        }

        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.PlayOneShot(randomClip);
    }
}
