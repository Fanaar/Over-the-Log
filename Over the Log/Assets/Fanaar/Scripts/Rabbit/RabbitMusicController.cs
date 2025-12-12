using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RabbitMusicController : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference musicEvent;
    public string parameterName = "MusicProgression";

    private EventInstance musicInstance;

    [Header("Drempels (aanpasbaar!)")]
    public int beginningThreshold = 0;
    public int midThreshold = 3;
    public int endThreshold = 7;   // Vanaf 7 → end

    private void Start()
    {
        // Start muziekevent
        musicInstance = RuntimeManager.CreateInstance(musicEvent);
        musicInstance.start();

        // Luister naar rabbit-collectie events
        RabbitManager.Instance.OnRabbitCollected += UpdateMusicState;
    }

    private void OnDestroy()
    {
        RabbitManager.Instance.OnRabbitCollected -= UpdateMusicState;
        musicInstance.release();
    }

    private void UpdateMusicState(int collected)
    {
        float valueToSet = 0f;

        if (collected >= endThreshold)
        {
            valueToSet = 2f;   // END
        }
        else if (collected >= midThreshold)
        {
            valueToSet = 1f;   // MID
        }
        else
        {
            valueToSet = 0f;   // BEGINNING
        }

        musicInstance.setParameterByName(parameterName, valueToSet);
        Debug.Log("FMOD Music parameter set to: " + valueToSet);
    }
}
