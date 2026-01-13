using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AmbienceMusic : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private StudioEventEmitter ambienceEmitter;

    void Start()
    {
        if (ambienceEmitter == null)
        {
            Debug.LogWarning("AmbienceStarter: Geen StudioEventEmitter gekoppeld.");
            return;
        }

        if (!ambienceEmitter.IsPlaying())
        {
            ambienceEmitter.Play();
        }
    }

    public void StopAmbience()
    {
        if (ambienceEmitter != null && ambienceEmitter.IsPlaying())
        {
            ambienceEmitter.Stop();
        }
    }
}
