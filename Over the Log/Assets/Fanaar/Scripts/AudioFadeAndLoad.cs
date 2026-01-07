using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioFadeAndLoad : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip clip;              // Voeg hier je clip in de inspector
    public float volume = 1f;

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    [Header("Scene")]
    public string nextSceneName;

    private bool hasStarted = false;

    private void Start()
    {
        fadeGroup.alpha = 0f;
        StartCoroutine(AudioSequence());
    }

    private IEnumerator AudioSequence()
    {
        if (hasStarted) yield break;
        hasStarted = true;

        // Maak een tijdelijke AudioSource
        AudioSource tempAudio = gameObject.AddComponent<AudioSource>();
        tempAudio.clip = clip;
        tempAudio.volume = volume;
        tempAudio.playOnAwake = false;

        tempAudio.Play();

        // Wacht tot audio klaar is
        yield return new WaitWhile(() => tempAudio.isPlaying);

        // Optioneel: verwijder de AudioSource na gebruik
        Destroy(tempAudio);

        // Fade naar zwart
        yield return StartCoroutine(FadeCanvasGroup(fadeGroup, 0f, 1f));

        // Laad de volgende scene
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        float timer = 0f;
        cg.alpha = from;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }

        cg.alpha = to;
    }
}
