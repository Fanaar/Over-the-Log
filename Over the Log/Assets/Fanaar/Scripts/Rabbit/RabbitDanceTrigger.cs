using UnityEngine;
using FMODUnity;

public class RabbitDanceTrigger : MonoBehaviour
{
    [Header("FMOD")]
    public StudioEventEmitter rabbitDanceEmitter;
    public RabbitMusicController musicController;

    [Header("Settings")]
    public float danceStateValue = 1f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        // Stop de muziek
        musicController?.StopMusic();

        if (rabbitDanceEmitter != null)
        {
            rabbitDanceEmitter.Play();
            rabbitDanceEmitter.SetParameter("DanceState", danceStateValue);
        }
    }

    public void StopDance()
    {
        rabbitDanceEmitter.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

    }

    public void StopCircleDance()
    {
        if (rabbitDanceEmitter != null)
        {
            rabbitDanceEmitter.Stop();
        }
    }

}