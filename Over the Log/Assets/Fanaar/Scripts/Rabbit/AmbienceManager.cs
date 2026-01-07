using UnityEngine;
using FMODUnity;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager Instance;

    [Header("FMOD Ambience Emitters")]
    public StudioEventEmitter ambienceIntro;
    public StudioEventEmitter eerieLoop;
    public StudioEventEmitter eerieEnding;

    [Header("Fade Settings")]
    public float quickFadeOutTime = 0.4f;
    public float normalFadeTime = 1.5f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        FadeAndLoadScene.OnSceneFadeStarted += HandleSceneFade;
    }

    private void OnDisable()
    {
        FadeAndLoadScene.OnSceneFadeStarted -= HandleSceneFade;
    }

    private void Start()
    {
        PlayIntroAmbience();
    }

    // ─────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────

    public void PlayIntroAmbience()
    {
        StopAllAmbienceWithFade();
        ambienceIntro?.Play();
    }

    public void FadeOutIntro()
    {
        if (ambienceIntro != null && ambienceIntro.IsPlaying())
            ambienceIntro.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void StartEerieLoop()
    {
        FadeOutIntro();
        if (eerieLoop != null && !eerieLoop.IsPlaying())
            eerieLoop.Play();
    }

    public void PlayEerieEnding()
    {
        StopAllAmbienceWithFade();
        if (eerieEnding != null)
            eerieEnding.Play();
    }

    public void QuickFadeOutAll()
    {
        StopAllAmbienceWithFade();
    }

    // ─────────────────────────────────────────
    // INTERNAL
    // ─────────────────────────────────────────

    private void StopAllAmbienceWithFade()
    {
        if (ambienceIntro != null && ambienceIntro.IsPlaying())
            ambienceIntro.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        if (eerieLoop != null && eerieLoop.IsPlaying())
            eerieLoop.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        if (eerieEnding != null && eerieEnding.IsPlaying())
            eerieEnding.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void HandleSceneFade()
    {
        QuickFadeOutAll();
    }
}
