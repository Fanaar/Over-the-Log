using UnityEngine;
using FMODUnity;

public class GrowlManager : MonoBehaviour
{
    public static GrowlManager Instance;

    [Header("Looping Growls")]
    public StudioEventEmitter lofiSneakyGrowl;               // Loop bij hond spawn
    public StudioEventEmitter heavyBreathing;               // Loop bij camera lock

    [Header("One-Shot Cues")]
    public StudioEventEmitter chaseSirenCue;                // One-shot bij camera lock
    public StudioEventEmitter animalsReverb;                // One-shot bij unlock controls
    public StudioEventEmitter wolfStartingChaseRoar;       // One-shot bij start chase

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

    // ───────── Public API ─────────

    public void PlayLofiSneakyGrowl() => lofiSneakyGrowl?.Play();
    public void StopLofiSneakyGrowl() => lofiSneakyGrowl?.Stop();

    public void PlayHeavyBreathing() => heavyBreathing?.Play();
    public void StopHeavyBreathing() => heavyBreathing?.Stop();


    public void PlayChaseSirenCue() => chaseSirenCue?.Play();
    public void PlayAnimalsReverb() => animalsReverb?.Play();
    public void PlayWolfStartingChaseRoar() => wolfStartingChaseRoar?.Play();
}
