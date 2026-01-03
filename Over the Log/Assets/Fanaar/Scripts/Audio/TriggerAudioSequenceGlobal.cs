using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerAudioSequenceGlobal : MonoBehaviour
{

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volume = 1f;

    private static int currentIndex = -1;
    private static bool canTrigger = true;

    private void Awake()
    {
        currentIndex = -1;
        canTrigger = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentIndex = -1;
        canTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!canTrigger)
            return;

        canTrigger = false;

        currentIndex++;

        if (currentIndex >= clips.Length)
            return;

        audioSource.clip = clips[currentIndex];
        audioSource.volume = volume;
        audioSource.Play();

        // Cooldown van 1 frame voorkomt dubbele triggers
        Invoke(nameof(ResetTriggerGate), 0.05f);
    }

    private void ResetTriggerGate()
    {
        canTrigger = true;
    }
}
