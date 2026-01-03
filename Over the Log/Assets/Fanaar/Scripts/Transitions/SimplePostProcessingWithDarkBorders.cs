using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SimplePostProcessingWithDarkBorders : MonoBehaviour
{
    [Header("Camera & Volumes")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Volume baseVolume;
    [SerializeField] private Volume volumeDarkBorders;

    [Header("Fade Settings")]
    [SerializeField] private float darkBordersFadeDuration = 1f;
    [SerializeField] private float darkBordersWeight = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine darkBordersCoroutine;
    private bool darkBordersActive = false;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
        UniversalAdditionalCameraData camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        camData.volumeLayerMask = ~0;

        if (baseVolume != null)
            baseVolume.weight = 1f;

        if (volumeDarkBorders != null)
            volumeDarkBorders.weight = 0f;
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
        float start = volumeDarkBorders.weight;
        float end = fadeIn ? darkBordersWeight : 0f;

        float t = 0f;

        while (t < darkBordersFadeDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / darkBordersFadeDuration);
            float smooth = fadeCurve.Evaluate(n);

            volumeDarkBorders.weight = Mathf.Lerp(start, end, smooth);
            yield return null;
        }

        volumeDarkBorders.weight = end;
        darkBordersCoroutine = null;
    }
}
