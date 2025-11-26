using UnityEngine;
using System.Collections;

public class DogCinematicManager : MonoBehaviour
{
    [Header("References")]
    public FirstPersonRabbitController playerController;
    public Transform playerCamera;
    public GameObject dog;

    [Header("Settings")]
    public float lookAtDogThreshold = 0.5f; // adjustable in inspector
    public float dogFocusDuration = 1.5f;   // adjustable in inspector
    public bool automaticCameraFocus = true;

    [Header("Dog Chase")]
    public float headStartDuration = 3f; // time player can move before dog chase starts
    public float dogSpawnDistance = 3f;  // <-- NEW inspector-adjustable spawn distance

    [Header("Cinematic Settings")]
    public float lookFreezeDuration = 1.5f; // seconds to freeze mouse look

    private bool cinematicStarted = false;

    public void StartCinematic()
    {
        if (cinematicStarted) return;
        cinematicStarted = true;
        StartCoroutine(CinematicSequence());
    }

    private IEnumerator CinematicSequence()
    {
        // Activate dog
        dog.SetActive(true);

        // Position dog behind player using the inspector value
        Vector3 spawnPos = playerController.transform.position - playerCamera.forward * dogSpawnDistance;
        spawnPos.y = playerController.transform.position.y;
        dog.transform.position = spawnPos;

        dog.transform.LookAt(playerController.transform.position);

        // Wait until player looks at the dog
        while (!PlayerLookingAtDog())
            yield return null;

        // Player is now looking at dog → freeze movement and optionally mouse look
        playerController.canMove = false;
        playerController.canLook = false;

        // Optional short freeze so player can “register” dog
        yield return new WaitForSeconds(lookFreezeDuration);

        // Re-enable movement and mouse look
        playerController.canMove = true;
        playerController.canLook = true;

        // Switch to Rabbit mode
        playerController.currentState = FirstPersonRabbitController.MovementState.Rabbit;
        Debug.Log("🐇 Player switched to Rabbit mode!");

        // Head start before dog chase
        yield return new WaitForSeconds(headStartDuration);

        // Start dog chase
        dog.GetComponent<DogController>()?.StartChase(playerController.transform);
        Debug.Log("🐶 Dog chase started!");
    }

    private bool PlayerLookingAtDog()
    {
        Vector3 cameraForward = playerCamera.forward;
        Vector3 directionToDog = (dog.transform.position - playerController.transform.position).normalized;
        float dot = Vector3.Dot(cameraForward, directionToDog);
        return dot >= lookAtDogThreshold;
    }
}
