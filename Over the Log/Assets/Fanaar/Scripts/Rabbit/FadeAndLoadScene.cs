using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class FadeAndLoadScene : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;
    public string sceneToLoad = "SceneName";

    [Header("Objects to deactivate")]
    public GameObject[] objectsToDeactivate;

    private bool isFading = false;

    // 🔔 Event voor andere systemen (audio, UI, analytics)
    public static event Action OnSceneFadeStarted;

    private void Start()
    {
        // Zorg dat het panel intern volledig opaque is
        Image img = fadeCanvasGroup.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        fadeCanvasGroup.alpha = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFading) return;

        if (!other.CompareTag("Player")) return;

        // 🔒 Voorkom dubbele triggers (meerdere colliders)
        GetComponent<Collider>().enabled = false;

        StartCoroutine(FadeAndLoad());
    }

    private System.Collections.IEnumerator FadeAndLoad()
    {
        isFading = true;

        // 🔔 Trigger event, andere systemen kunnen hierop reageren
        OnSceneFadeStarted?.Invoke();

        // ✅ Objecten deactiveren
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
