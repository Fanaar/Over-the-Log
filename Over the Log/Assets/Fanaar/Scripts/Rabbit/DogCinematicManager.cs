using UnityEngine;
using System.Collections;

public class DogCinematicManager : MonoBehaviour
{
    [Header("References")]
    public FirstPersonRabbitController playerController;
    public Transform playerCamera;
    public GameObject dog;

    [Header("Settings")]
    public float lookAtDogThreshold = 0.5f;
    public float dogFocusDuration = 1.5f;
    public bool automaticCameraFocus = true;

    [Header("Dog Chase")]
    public float headStartDuration = 3f;
    public float dogSpawnDistance = 3f;
    public float dogSpawnHeightOffset = 1.5f; // NEW

    [Header("Cinematic Settings")]
    public float lookFreezeDuration = 1.5f;

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

        // Calculate spawn position behind player
        Vector3 spawnPos = playerController.transform.position - playerCamera.forward * dogSpawnDistance;

        // RAYCAST DOWN to terrain so the dog isn't underground
        Vector3 rayOrigin = spawnPos + Vector3.up * 5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
        {
            spawnPos.y = hit.point.y + dogSpawnHeightOffset;
        }
        else
        {
            // fallback if no terrain found
            spawnPos.y = playerController.transform.position.y + dogSpawnHeightOffset;
        }

        dog.transform.position = spawnPos;
        dog.transform.LookAt(playerController.transform.position);

        // Wait until player looks at the dog
        while (!PlayerLookingAtDog())
            yield return null;

        playerController.canMove = false;
        playerController.canLook = false;

        yield return new WaitForSeconds(lookFreezeDuration);

        playerController.canMove = true;
        playerController.canLook = true;

        playerController.currentState = FirstPersonRabbitController.MovementState.Rabbit;
        Debug.Log("🐇 Player switched to Rabbit mode!");

        yield return new WaitForSeconds(headStartDuration);

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
