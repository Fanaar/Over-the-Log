using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RabbitMusicController : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicEvent;             // Sleep hier je FMOD event
    public string parameterName = "rabbits collected";

    private EventInstance musicInstance;

    [Header("Drempels")]
    public int beginningThreshold = 0;
    public int midThreshold = 3;
    public int endThreshold = 7;

    private void Start()
    {
        // 1) Maak één instance en start hem
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();

        // 2) Luister naar RabbitManager
        RabbitManager.Instance.OnRabbitCollected += UpdateMusicState;
    }

    private void OnDestroy()
    {
        // Stop luisteren en release instance
        if (RabbitManager.Instance != null)
            RabbitManager.Instance.OnRabbitCollected -= UpdateMusicState;

        musicInstance.release();
    }

    private void UpdateMusicState(int collected)
    {
        float valueToSet = 0f;

        if (collected >= endThreshold)
            valueToSet = 2f;
        else if (collected >= midThreshold)
            valueToSet = 1f;
        else
            valueToSet = 0f;

        // Zet de parameter op de bestaande instance
        musicInstance.setParameterByName(parameterName, valueToSet);

        Debug.Log("FMOD parameter set to: " + valueToSet);
    }
}
