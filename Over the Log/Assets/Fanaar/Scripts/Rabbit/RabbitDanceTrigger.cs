using UnityEngine;
using FMODUnity;

public class RabbitDanceTrigger : MonoBehaviour
{
    [Header("FMOD")]
    public StudioEventEmitter rabbitDanceEmitter;
    public RabbitMusicController musicController;

    [Header("Trigger")]
    [SerializeField] public Collider triggerCollider;

    private bool hasStartedDance = false;
    private bool hasStoppedDance = false;

    private void Awake()
    {
        // Trigger start UIT
        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerCollider.enabled) return;
        if (hasStartedDance) return;
        if (!other.CompareTag("Player")) return;

        hasStartedDance = true;

        // Stop eerdere muziek
        musicController?.StopMusic();

        // Start dance muziek (1x)
        rabbitDanceEmitter?.Play();

        Debug.Log("🐇 Dance circle music STARTED");
    }

    public void EnableTrigger()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }

    public void StopCircleDance()
    {
        if (hasStoppedDance) return;

        hasStoppedDance = true;
        rabbitDanceEmitter?.Stop();

        Debug.Log("🛑 Dance circle music STOPPED");
    }
}
