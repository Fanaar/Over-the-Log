using UnityEngine;
using System;
using FMODUnity;
using FMOD.Studio;

public class RabbitManager : MonoBehaviour
{
    public static RabbitManager Instance;

    [Header("Rabbit Count")]
    public int totalRabbits = 10;
    public int collectedRabbits = 0;
    public bool allRabbitsCollected = false;

    [Header("References")]
    public RabbitMusicController rabbitMusicController;
    public GameObject voiceLine;

    [Header("FMOD – Dance Music")]
    public StudioEventEmitter rabbitDanceEmitter;

    public event Action<int> OnRabbitCollected;
    public event Action OnAllRabbitsCollected;

    private bool hasStoppedDanceMusic = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterRabbitActivated()
    {
        if (allRabbitsCollected) return;

        collectedRabbits++;
        Debug.Log("🐇 Rabbit collected! Total: " + collectedRabbits);

        OnRabbitCollected?.Invoke(collectedRabbits);

        if (collectedRabbits >= totalRabbits)
        {
            allRabbitsCollected = true;

            Debug.Log("🎉 ALL RABBITS COLLECTED");

            // 🛑 Stop huidige (exploration / chase) muziek
            rabbitMusicController?.StopMusic();

            // 🎮 Gameplay
            if (voiceLine != null)
                voiceLine.SetActive(true);

            OnAllRabbitsCollected?.Invoke();
            rabbitDanceEmitter.Play();
        }
    }

    /// <summary>
    /// Stopt expliciet de dance-circle muziek.
    /// Wordt aangeroepen door RabbitDanceManager (niet door triggers).
    /// </summary>
    public void StopCircleDance()
    {
        if (hasStoppedDanceMusic) return;
        hasStoppedDanceMusic = true;

        if (rabbitDanceEmitter != null)
        {
            rabbitDanceEmitter.Stop();
            Debug.Log("🛑 Dance circle music STOPPED (via RabbitManager)");
        }
        else
        {
            Debug.LogWarning("RabbitManager: rabbitDanceEmitter not assigned!");
        }
    }
}
