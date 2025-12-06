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
    public float dogSpawnHeightOffset = 1.5f;

    [Header("Cinematic Camera")]
    public Transform dogLookTarget;        // Drag a target (dog head position)
    public float cameraLerpSpeed = 2f;      // Higher = faster rotation
    public float lookFreezeDuration = 1.5f;

    [Header("Cinematic Extra Event")]
    public GameObject objectToActivate;
    public GameObject objectToActivate2;

    [Header("Dog Animator")]
    public Animator dogAnimator;


    // ⭐ NEW: Third object activated AFTER freeze
    public GameObject objectToActivateAfterFreeze;

    private bool cinematicStarted = false;

    public void StartCinematic()
    {
        if (cinematicStarted) return;
        cinematicStarted = true;
        StartCoroutine(CinematicSequence());
    }

    private IEnumerator CinematicSequence()
    {
        // --- Spawn dog ---
        dog.SetActive(true);

        // Calculate spawn position behind player
        Vector3 spawnPos = playerController.transform.position - playerCamera.forward * dogSpawnDistance;

        // Raycast down to terrain so dog isn't underground
        Vector3 rayOrigin = spawnPos + Vector3.up * 5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
            spawnPos.y = hit.point.y + dogSpawnHeightOffset;
        else
            spawnPos.y = playerController.transform.position.y + dogSpawnHeightOffset;

        dog.transform.position = spawnPos;
        dog.transform.LookAt(playerController.transform.position);

        // --- WAIT until the player naturally looks close enough ---
        while (!PlayerLookingAtDog())
            yield return null;

        // --- Freeze controls ---
        playerController.canMove = false;
        playerController.canLook = false;

        // --- Smoothly rotate camera toward target ---
        if (automaticCameraFocus && dogLookTarget != null)
        {
            float elapsed = 0f; // Correct scope

            while (elapsed < dogFocusDuration)
            {
                Vector3 dir = (dogLookTarget.position - playerCamera.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(dir);

                playerCamera.rotation = Quaternion.Slerp(
                    playerCamera.rotation,
                    targetRot,
                    Time.deltaTime * cameraLerpSpeed
                );

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap exactly at the end to avoid tiny offset
            playerCamera.LookAt(dogLookTarget);

            // Activate extra objects
            if (objectToActivate != null)
                objectToActivate.SetActive(true);
            if (objectToActivate2 != null)
                objectToActivate2.SetActive(true);

            // 🔥 Trigger Scary Face animation
            if (dogAnimator != null)
                dogAnimator.SetTrigger("ScaryFace");
        }

        // Hold camera frozen for dramatic effect
        yield return new WaitForSeconds(lookFreezeDuration);

        // 🔥 Activate third object AFTER freeze
        if (objectToActivateAfterFreeze != null)
            objectToActivateAfterFreeze.SetActive(true);

        // --- Resume player control ---
        playerController.canMove = true;
        playerController.canLook = true;
        playerController.currentState = FirstPersonRabbitController.MovementState.Rabbit;

        Debug.Log("🐇 Player switched to Rabbit mode!");

        // --- Give dog a head start ---
        yield return new WaitForSeconds(headStartDuration);

        // 🔥 Trigger Run animation
        if (dogAnimator != null)
            dogAnimator.SetTrigger("Run");

        // Start chasing player
        dog.GetComponent<DogController>()?.StartChase(playerController.transform);
        Debug.Log("🐶 Dog chase started!");
    }


    private bool PlayerLookingAtDog()
    {
        Vector3 cameraForward = playerCamera.forward;
        Vector3 directionToDog = (dog.transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(cameraForward, directionToDog);
        return dot >= lookAtDogThreshold;
    }
}
