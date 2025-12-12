using UnityEngine;
using System;

public class RabbitManager : MonoBehaviour
{
    public static RabbitManager Instance;

    public int totalRabbits = 10;        // Hoeveel konijnen er bestaan
    public int collectedRabbits = 0;     // Hoeveel er geactiveerd zijn

    public event Action<int> OnRabbitCollected;
    // Event → handig voor later (FMOD of UI)

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterRabbitActivated()
    {
        collectedRabbits++;

        Debug.Log("Rabbit collected! Total: " + collectedRabbits);

        OnRabbitCollected?.Invoke(collectedRabbits);
    }
}
