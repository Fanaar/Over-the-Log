using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RabbitMusicController : MonoBehaviour
{
    [Header("FMOD")]
    public StudioEventEmitter musicEmitter;

    private bool isStopping = false;

    private void Start()
    {
        if (musicEmitter == null)
        {
            Debug.LogWarning("RabbitMusicController: musicEmitter not assigned!");
            return;
        }

        musicEmitter.Play();
    }

    public void StopMusic()
    {
        if (isStopping) return;
        isStopping = true;

        musicEmitter.Stop();
        Debug.Log("🎵 Rabbit music STOP (fade)");
    }
}
