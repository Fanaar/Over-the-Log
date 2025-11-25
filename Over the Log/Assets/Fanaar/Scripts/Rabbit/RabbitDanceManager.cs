using UnityEngine;

public class RabbitDanceManager : MonoBehaviour
{
    [Header("Rabbits")]
    public RabbitController[] rabbits;
    public float rotationSpeed = 30f;

    [Header("Rounds Settings")]
    public int roundsBeforeRunAway = 1; // Aantal rondes voordat konijnen wegrennen

    [Header("Run Away Settings")]
    public bool useRandomBetweenTransforms = true;
    public Transform runStart;  // Beginpunt van runaway
    public Transform runEnd;    // Eindpunt van runaway

    private float currentAngle = 0f;
    private int completedRotations = 0;
    private bool allReady = false;
    private bool hasRunAway = false;

    void Update()
    {
        if (!allReady)
        {
            // Check of alle konijnen op hun plek staan
            allReady = true;
            foreach (var rabbit in rabbits)
            {
                if (!rabbit.IsAtDanceSpot)
                {
                    allReady = false;
                    break;
                }
            }
            return;
        }

        // Alleen als ALLE konijnen klaar zijn → draaien & tellen
        currentAngle += rotationSpeed * Time.deltaTime;

        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
            completedRotations++;
            Debug.Log("🎉 Cirkel compleet! Totaal rondes: " + completedRotations);
        }

        // Check of ze weg moeten rennen
        if (!hasRunAway && completedRotations >= roundsBeforeRunAway)
        {
            foreach (var rabbit in rabbits)
            {
                Vector3 targetDir;

                if (useRandomBetweenTransforms && runStart != null && runEnd != null)
                {
                    // Kies een random positie tussen runStart en runEnd
                    Vector3 randomPos = Vector3.Lerp(runStart.position, runEnd.position, Random.value);
                    targetDir = (randomPos - rabbit.transform.position).normalized;
                }
                else
                {
                    // Default vaste richting
                    targetDir = Vector3.forward;
                }

                rabbit.RunAway(targetDir);
            }

            hasRunAway = true;
            Debug.Log("🐇 Konijnen gaan nu wegrennen!");
        }
    }

    public int GetRotationCount()
    {
        return completedRotations;
    }
}
