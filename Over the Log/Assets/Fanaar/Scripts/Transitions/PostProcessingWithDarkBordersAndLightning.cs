using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingWithDarkBordersAndLightning : MonoBehaviour
{
    [Header("Camera & Volumes")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Volume baseVolume;
    [SerializeField] private Volume volumeDarkBorders;
    [SerializeField] private Volume volumeLightning;

    [Header("Fade Settings")]
    [SerializeField] private float darkBordersFadeDuration = 1f;
    [SerializeField] private float lightningFadeDuration = 0.5f;
    [SerializeField] private float darkBordersWeight = 1f;
    [SerializeField] private float lightningWeight = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine darkBordersCoroutine;
    private Coroutine lightningCoroutine;
    private bool darkBordersActive = false;
    private bool lightningActive = false;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
        UniversalAdditionalCameraData camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        camData.volumeLayerMask = ~0;

        if (baseVolume != null) baseVolume.weight = 1f;
        if (volumeDarkBorders != null) volumeDarkBorders.weight = 0f;
        if (volumeLightning != null) volumeLightning.weight = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DarkBorders"))
            SetDarkBorders(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DarkBorders"))
            SetDarkBorders(false);
    }

    // 🌩 Wordt extern aangeroepen
    public void TriggerLightning(bool active)
    {
        if (lightningActive == active || volumeLightning == null)
            return;

        lightningActive = active;

        if (lightningCoroutine != null)
            StopCoroutine(lightningCoroutine);

        lightningCoroutine = StartCoroutine(FadeLightning(active));
    }

    private void SetDarkBorders(bool active)
    {
        if (darkBordersActive == active || volumeDarkBorders == null)
            return;

        darkBordersActive = active;

        if (darkBordersCoroutine != null)
            StopCoroutine(darkBordersCoroutine);

        darkBordersCoroutine = StartCoroutine(FadeDarkBorders(active));
    }

    private IEnumerator FadeDarkBorders(bool fadeIn)
    {
        yield return FadeVolume(volumeDarkBorders, fadeIn, darkBordersFadeDuration);
        darkBordersCoroutine = null;
    }

    private IEnumerator FadeLightning(bool fadeIn)
    {
        yield return FadeVolume(volumeLightning, fadeIn, lightningFadeDuration);
        lightningCoroutine = null;
    }

    private IEnumerator FadeVolume(Volume volume, bool fadeIn, float duration)
    {
        float start = volume.weight;
        float end = fadeIn ? 1f : 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);
            float smooth = fadeCurve.Evaluate(n);

            volume.weight = Mathf.Lerp(start, end, smooth);
            yield return null;
        }

        volume.weight = end;
    }
}
