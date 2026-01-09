using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RabbitMusicController : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicEvent;

    private EventInstance musicInstance;
    private bool isPlaying = false;

    private void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();
        isPlaying = true;
    }

    public void StopMusic()
    {
        if (!isPlaying) return;

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        isPlaying = false;
    }

    private void OnDestroy()
    {
        if (isPlaying)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}
