using UnityEngine;
using FMODUnity;
using System.Collections;

public class RabbitDanceAudioManager : MonoBehaviour
{
    [Header("FMOD 3D Emitters")]
    public StudioEventEmitter circleForming; // One-shot
    public StudioEventEmitter circleDance;   // Loop

    [Header("Dance Manager Reference")]
    public RabbitDanceManager danceManager; // Sleep hier je DanceManager in inspector

    private bool formingPlayed = false;
    private bool danceStarted = false;

    void Update()
    {
        // Check of de cirkel compleet is en we nog niet gespeeld hebben
        if (!formingPlayed && danceManager != null && danceManager.AllReady)
        {
            // Speel RabbitCircleForming one-shot
            circleForming?.Play();
            formingPlayed = true;

            // Start loop zodra forming klaar is
            StartCoroutine(StartDanceAfterForming());
        }
    }

    private IEnumerator StartDanceAfterForming()
    {
        if (circleForming == null) yield break;

        // Wacht tot forming event klaar is
        while (circleForming.IsPlaying())
            yield return null;

        if (circleDance != null)
        {
            circleDance.Play();
            danceStarted = true;
        }
    }

    // Wordt aangeroepen vanuit RabbitDanceManager.StartRunAway()
    public void StopDance()
    {
        if (circleDance != null && danceStarted)
        {
            circleDance.Stop();
            danceStarted = false;
        }
    }
}
