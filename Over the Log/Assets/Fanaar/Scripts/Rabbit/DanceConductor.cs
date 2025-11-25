using UnityEngine;
using System;

public class DanceConductor : MonoBehaviour
{
    public static event Action OnBeat;

    public float beatInterval = 2f; // tijd tussen dans-synchronisatie
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= beatInterval)
        {
            timer = 0f;
            OnBeat?.Invoke();
        }
    }
}
