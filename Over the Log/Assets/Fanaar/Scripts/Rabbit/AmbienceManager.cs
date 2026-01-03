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
        StopAllAmbienceImmediate();
        ambienceIntro?.Play();
    }

    public void FadeOutIntro()
    {
        if (ambienceIntro != null && ambienceIntro.IsPlaying())
            ambienceIntro.Stop(); // FMOD emitter fade out settings zorgen voor smooth fade
    }

    public void StartEerieLoop()
    {
        FadeOutIntro();
        if (eerieLoop != null && !eerieLoop.IsPlaying())
            eerieLoop.Play();
    }

    public void PlayEerieEnding()
    {
        // Stop alles anders behalve deze
        StopAllAmbienceImmediate();
        if (eerieEnding != null)
            eerieEnding.Play();
    }

    public void QuickFadeOutAll()
    {
        StopAllAmbienceImmediate();
    }

    // ─────────────────────────────────────────
    // INTERNAL
    // ─────────────────────────────────────────

    private void StopAllAmbienceImmediate()
    {
        if (ambienceIntro != null && ambienceIntro.IsPlaying()) ambienceIntro.Stop();
        if (eerieLoop != null && eerieLoop.IsPlaying()) eerieLoop.Stop();
        if (eerieEnding != null && eerieEnding.IsPlaying()) eerieEnding.Stop();
    }

    private void HandleSceneFade()
    {
        QuickFadeOutAll();
    }
}
