using UnityEngine;

public class RabbitDanceManager : MonoBehaviour
{
    [Header("Rabbits")]
    public RabbitController[] rabbits;
    public float rotationSpeed = 30f;

    [Header("Rounds Settings")]
    public int roundsBeforeRunAway = 1;

    [Header("Run Away Settings")]
    public Transform runStart;  // Beginpunt renrichting
    public Transform runEnd;    // Eindpunt renrichting
    public bool useRandomBetweenTransforms = true;

    [Header("Dog Settings")]
    public GameObject dog;          // bestaande hond in scene
    public Transform player;        // player transform
    public float dogSpawnDistance = 3f;

    [Header("Player Look Settings")]
    [Range(-1f, 1f)]
    public float lookDotThreshold = -0.5f; // adjust in inspector

    private float currentAngle = 0f;
    private int completedRotations = 0;
    private bool allReady = false;
    private bool hasRunAway = false;
    private bool dogActivated = false;

    void Update()
    {
        // Check of alle konijnen op hun plek staan
        if (!allReady)
        {
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

        // Draai de cirkel
        if (!hasRunAway)
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle >= 360f)
            {
                currentAngle -= 360f;
                completedRotations++;
                Debug.Log("🎉 Cirkel compleet! Rondes: " + completedRotations);
            }
        }

        // Check of konijnen weg mogen rennen
        if (!hasRunAway && completedRotations >= roundsBeforeRunAway && PlayerLookingAtRunDirection())
        {
            StartRunAway();
            ActivateDogBehindPlayer();
        }
    }

    private void StartRunAway()
    {
        foreach (var rabbit in rabbits)
        {
            Vector3 targetDir;

            if (useRandomBetweenTransforms && runStart != null && runEnd != null)
            {
                Vector3 randomPos = Vector3.Lerp(runStart.position, runEnd.position, Random.value);
                targetDir = (randomPos - rabbit.transform.position).normalized;
            }
            else
            {
                targetDir = Vector3.forward;
            }

            rabbit.RunAway(targetDir);
        }

        hasRunAway = true;
        Debug.Log("🐇 Konijnen rennen weg!");
    }

    private void ActivateDogBehindPlayer()
    {
        if (dogActivated || dog == null) return;

        dog.SetActive(true);

        // Positioneer hond achter speler
        Vector3 spawnPos = player.position - Camera.main.transform.forward * dogSpawnDistance;
        spawnPos.y = player.position.y; // zelfde hoogte
        dog.transform.position = spawnPos;

        // Laat hond naar speler kijken
        dog.transform.LookAt(player);

        dogActivated = true;
        Debug.Log("🐶 Hond geactiveerd achter speler!");
    }

    // Check of speler kijkt richting de renrichting
    private bool PlayerLookingAtRunDirection()
    {
        if (runStart == null || runEnd == null) return true;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 runDirection = (runEnd.position - runStart.position).normalized;

        float dot = Vector3.Dot(cameraForward, runDirection);
        return dot < lookDotThreshold; // now adjustable
    }

    // Optioneel: externe methode om rondes te checken
    public int GetCompletedRotations()
    {
        return completedRotations;
    }
}
