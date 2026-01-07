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

    [Header("Third Volume (External Control)")]
    [SerializeField] private Volume thirdVolume;
    [SerializeField] private float thirdVolumeFadeDuration = 1f;
    [SerializeField] private float thirdVolumeTargetWeight = 1f;

    [Header("Fade Settings")]
    [SerializeField] private float darkBordersFadeDuration = 1f;
    [SerializeField] private float darkBordersWeight = 1f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine darkBordersCoroutine;
    private Coroutine thirdVolumeCoroutine;

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

        if (thirdVolume != null)
            thirdVolume.weight = 0f;
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

        darkBordersCoroutine = StartCoroutine(FadeVolume(volumeDarkBorders, active ? darkBordersWeight : 0f, darkBordersFadeDuration));
    }

    // ===== Public API for third volume =====

    public void FadeThirdVolumeIn()
    {
        if (thirdVolume == null) return;

        if (thirdVolumeCoroutine != null)
            StopCoroutine(thirdVolumeCoroutine);

        thirdVolumeCoroutine = StartCoroutine(FadeVolume(thirdVolume, thirdVolumeTargetWeight, thirdVolumeFadeDuration));
    }

    public void FadeThirdVolumeOut()
    {
        if (thirdVolume == null) return;

        if (thirdVolumeCoroutine != null)
            StopCoroutine(thirdVolumeCoroutine);

        thirdVolumeCoroutine = StartCoroutine(FadeVolume(thirdVolume, 0f, thirdVolumeFadeDuration));
    }

    // ===== Generic fade =====

    private IEnumerator FadeVolume(Volume volume, float target, float duration)
    {
        float start = volume.weight;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);
            float smooth = fadeCurve.Evaluate(n);

            volume.weight = Mathf.Lerp(start, target, smooth);
            yield return null;
        }

        volume.weight = target;
    }
}
