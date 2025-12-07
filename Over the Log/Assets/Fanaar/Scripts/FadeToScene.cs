using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class FadeToScene : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;
    public string sceneToLoad = "GameScene";

    private bool isFading = false;

    void Update()
    {
        if (!isFading && Input.anyKeyDown)
        {
            StartCoroutine(FadeOutAndLoad());
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        isFading = true;
        float time = 0;

        while (time < fadeDuration)
        {
            fadePanel.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fadePanel.alpha = 1f;

        SceneManager.LoadScene(sceneToLoad);
    }
}
