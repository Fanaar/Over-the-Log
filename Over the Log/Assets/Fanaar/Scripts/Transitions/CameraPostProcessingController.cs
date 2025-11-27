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

    [SerializeField] private Volume volumeUnderTheTrees;
    [SerializeField] private Volume volumeAboveTheTrees;
    [SerializeField] private Volume volumeReflect;

    [Header("Fog instellingen")]
    [SerializeField] private bool controlFog = true;
    [SerializeField] private float fogUnderTheTrees = 0.02f;
    [SerializeField] private float fogAboveTheTrees = 0f;
    [SerializeField] private float fogReflect = 0f;

    [Header("Transitie instellingen")]
    [SerializeField] private float transitionDuration = 2f;

    [Header("Fade Smoothing")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Trigger Objects (drag your colliders here)")]
    [SerializeField] private Collider flyingTrigger1;
    [SerializeField] private Collider flyingTrigger2;

    private State currentState = State.UnderTheTrees;
    private Coroutine transitionCoroutine;

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
        // Camera sees ALL volumes
        UniversalAdditionalCameraData camData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        camData.volumeLayerMask = ~0; // every layer

        SetStateInstant(State.UnderTheTrees);
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
    }

    private void ChangeState(State newState)
    {
        if (newState == currentState)
            return;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionToState(newState));
    }

    private void SetStateInstant(State state)
    {
        volumeUnderTheTrees.weight = (state == State.UnderTheTrees) ? 1f : 0f;
        volumeAboveTheTrees.weight = (state == State.AboveTheTrees) ? 1f : 0f;
        volumeReflect.weight = (state == State.Reflect) ? 1f : 0f;

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
            float normalized = Mathf.Clamp01(t / transitionDuration);
            float smooth = fadeCurve.Evaluate(normalized);

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
            case State.UnderTheTrees: return volumeUnderTheTrees;
            case State.AboveTheTrees: return volumeAboveTheTrees;
            case State.Reflect: return volumeReflect;
        }
        return null;
    }

    private float GetFogForState(State state)
    {
        switch (state)
        {
            case State.UnderTheTrees: return fogUnderTheTrees;
            case State.AboveTheTrees: return fogAboveTheTrees;
            case State.Reflect: return fogReflect;
        }
        return fogUnderTheTrees;
    }
}
