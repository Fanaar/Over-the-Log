using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ImageSlideshow : MonoBehaviour
{
    public CanvasGroup[] images;
    public float fadeDuration = 1f;
    public string nextSceneName;

    private int currentIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        // Fade in the very first image at the start
        StartCoroutine(FadeCanvasGroup(images[0], 0f, 1f));
    }

    public void OnButtonPressed()
    {
        if (!isTransitioning)
            StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        isTransitioning = true;

        CanvasGroup current = images[currentIndex];

        // Fade out current image
        yield return StartCoroutine(FadeCanvasGroup(current, current.alpha, 0f));

        currentIndex++;

        if (currentIndex >= images.Length)
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        CanvasGroup next = images[currentIndex];

        // Fade in next image
        yield return StartCoroutine(FadeCanvasGroup(next, 0f, 1f));

        isTransitioning = false;
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
