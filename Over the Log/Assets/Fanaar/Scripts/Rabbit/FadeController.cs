using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup fadePanel;

    [Header("Timings")]
    public float fadeInDuration = 1f;
    public float visibleDuration = 3f;
    public float fadeOutDuration = 1f;

    [Header("Scene")]
    public string sceneToLoad;

    void Start()
    {
        if (fadePanel == null)
        {
            Debug.LogError("FadeController: No fadePanel assigned!");
            return;
        }

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1f; // start black

        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        // Fade in
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        // Stay visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        // Load scene (optional)
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            fadePanel.alpha = Mathf.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = to;
    }
}
