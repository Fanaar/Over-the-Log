using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerAudioOnceGlobally : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;

    private static bool hasPlayedGlobal = false;

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
        hasPlayedGlobal = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasPlayedGlobal && other.CompareTag("Player"))
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();

            hasPlayedGlobal = true;
        }
    }
}
