using System.Collections;
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

    // Lokaal per trigger
    private bool used = false;

    // Event wanneer sequence klaar is
    public static System.Action OnSequenceFinished;

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
        if (used)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (!canTrigger)
            return;

        used = true;
        canTrigger = false;

        currentIndex++;

        if (currentIndex < clips.Length)
        {
            audioSource.clip = clips[currentIndex];
            audioSource.volume = volume;
            audioSource.Play();

            // Als dit de laatste clip is → wacht tot hij klaar is
            if (currentIndex == clips.Length - 1)
                StartCoroutine(WaitForLastClip());
        }

        Invoke(nameof(ResetTriggerGate), 0.05f);
    }

    private IEnumerator WaitForLastClip()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        OnSequenceFinished?.Invoke();
    }

    private void ResetTriggerGate()
    {
        canTrigger = true;
    }
}
