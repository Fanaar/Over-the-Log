using UnityEngine;
using System.Collections;

public class FadeInFromBlack : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float time = 0;

        while (time < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);
    }
}
