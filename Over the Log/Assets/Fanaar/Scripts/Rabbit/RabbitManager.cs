using UnityEngine;
using System;

public class RabbitManager : MonoBehaviour
{
    public static RabbitManager Instance;

    [Header("Rabbit Count")]
    public int totalRabbits = 10;
    public int collectedRabbits = 0;

    public RabbitDanceTrigger rabbitDanceTrigger;
    public bool allRabbitsCollected = false;

    public event Action<int> OnRabbitCollected;
    public event Action OnAllRabbitsCollected;
    public GameObject voiceLine;

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
            rabbitDanceTrigger.EnableTrigger();
            voiceLine.SetActive(true);
            allRabbitsCollected = true;
            Debug.Log("🎉 ALL RABBITS COLLECTED");
            OnAllRabbitsCollected?.Invoke();
        }
    }
}
