using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeToScene : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    public string sceneToLoad = "GameScene";

    [Header("Audio Settings")]
    public AudioSource musicSource;   // Achtergrondmuziek
    public AudioSource sfxSource;     // Voor click geluid
    public AudioClip clickSound;
    public float musicFadeDuration = 1f;  // Hoelang het uitfaden van de muziek duurt

    private bool isFading = false;

    void Start()
    {
        if (musicSource)
            musicSource.Play(); // Start achtergrondmuziek meteen
    }

    void Update()
    {
        if (!isFading && Input.anyKeyDown)
        {
            // Speel click geluid
            if (sfxSource && clickSound)
            {
                sfxSource.PlayOneShot(clickSound);
            }

            StartCoroutine(FadeOutSceneAndMusic());
        }
    }

    private IEnumerator FadeOutSceneAndMusic()
    {
        isFading = true;
        float time = 0f;

        // Sla de originele muziekvolume op
        float originalVolume = musicSource ? musicSource.volume : 1f;

        while (time < fadeDuration || (musicSource && time < musicFadeDuration))
        {
            // Fade canvas
            if (fadePanel)
                fadePanel.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);

            // Fade muziek
            if (musicSource)
                musicSource.volume = Mathf.Lerp(originalVolume, 0f, time / musicFadeDuration);

            time += Time.deltaTime;
            yield return null;
        }

        // Zorg dat alles compleet is gefaded
        if (fadePanel) fadePanel.alpha = 1f;
        if (musicSource) musicSource.volume = 0f;

        SceneManager.LoadScene(sceneToLoad);
    }
}
