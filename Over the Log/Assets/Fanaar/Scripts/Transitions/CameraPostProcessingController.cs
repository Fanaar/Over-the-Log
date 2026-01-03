using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraPostProcessingController : MonoBehaviour
{
    public enum State
    {
        UnderTheTrees,
        AboveTheTrees,
        Reflect
    }

    [Header("Camera & Volumes")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Volume volumeChildLikeWonder;
    [SerializeField] private Volume volumeDollyZoom;
    [SerializeField] private Volume volumeAction;

    [Header("Dark Borders Overlay")]
    [SerializeField] private Volume volumeDarkBorders;
    [SerializeField] private float darkBordersWeight = 1f;

    [Header("Fog instellingen")]
    [SerializeField] private bool controlFog = true;
    [SerializeField] private float fogNormal = 0.02f;
    [SerializeField] private float fogDolly = 0f;
    [SerializeField] private float fogAction = 0f;

    [Header("Transitie instellingen")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private float darkBordersFadeDuration = 1f;

    [Header("Fade Smoothing")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Trigger Objects")]
    [SerializeField] private Collider flyingTrigger1;
    [SerializeField] private Collider flyingTrigger2;

    private State currentState = State.UnderTheTrees;
    private Coroutine transitionCoroutine;
    private Coroutine darkBordersCoroutine;
    private bool darkBordersActive = false;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        UniversalAdditionalCameraData camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        camData.volumeLayerMask = ~0;

        SetStateInstant(State.UnderTheTrees);
        if (volumeDarkBorders != null)
            volumeDarkBorders.weight = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == flyingTrigger1)
        {
            ChangeState(State.AboveTheTrees);
        }
        else if (other == flyingTrigger2)
        {
            ChangeState(State.Reflect);
        }
        else if (other.CompareTag("DarkBorders"))
        {
            SetDarkBorders(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DarkBorders"))
        {
            SetDarkBorders(false);
        }
    }

    private void ChangeState(State newState)
    {
        if (newState == currentState)
            return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionToState(newState));
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

    private void SetStateInstant(State state)
    {
        volumeChildLikeWonder.weight = (state == State.UnderTheTrees) ? 1f : 0f;
        volumeDollyZoom.weight = (state == State.AboveTheTrees) ? 1f : 0f;
        volumeAction.weight = (state == State.Reflect) ? 1f : 0f;

        if (controlFog)
            RenderSettings.fogDensity = GetFogForState(state);

        currentState = state;
    }

    private IEnumerator TransitionToState(State targetState)
    {
        Volume fromVolume = GetVolumeForState(currentState);
        Volume toVolume = GetVolumeForState(targetState);

        float startFog = controlFog ? RenderSettings.fogDensity : 0f;
        float endFog = controlFog ? GetFogForState(targetState) : 0f;

        if (fromVolume != null) fromVolume.weight = 1f;
        if (toVolume != null) toVolume.weight = 0f;

        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / transitionDuration);
            float smooth = fadeCurve.Evaluate(n);

            if (fromVolume != null) fromVolume.weight = 1f - smooth;
            if (toVolume != null) toVolume.weight = smooth;

            if (controlFog)
                RenderSettings.fogDensity = Mathf.Lerp(startFog, endFog, smooth);

            yield return null;
        }

        if (fromVolume != null) fromVolume.weight = 0f;
        if (toVolume != null) toVolume.weight = 1f;

        if (controlFog)
            RenderSettings.fogDensity = endFog;

        currentState = targetState;
        transitionCoroutine = null;
    }

    private Volume GetVolumeForState(State state)
    {
        switch (state)
        {
            case State.UnderTheTrees: return volumeChildLikeWonder;
            case State.AboveTheTrees: return volumeDollyZoom;
            case State.Reflect: return volumeAction;
        }
        return null;
    }

    private float GetFogForState(State state)
    {
        switch (state)
        {
            case State.UnderTheTrees: return fogNormal;
            case State.AboveTheTrees: return fogDolly;
            case State.Reflect: return fogAction;
        }
        return fogNormal;
    }
}
