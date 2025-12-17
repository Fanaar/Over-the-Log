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
    public Transform dogLookTarget;
    public float cameraLerpSpeed = 2f;
    public float lookFreezeDuration = 1.5f;

    [Header("Cinematic Extra Event")]
    public GameObject objectToActivate;
    public GameObject objectToActivate2;

    [Header("Dog Animator")]
    public Animator dogAnimator;

    // Third object activated AFTER freeze
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
        // --- 1. Spawn dog ---
        dog.SetActive(true);

        // 🔔 AUDIO: Fade out normal ambience (korte stilte)
        AmbienceManager.Instance?.FadeOutIntro();
        // 🔔 AUDIO: Start sneaky growl loop
        GrowlManager.Instance?.PlayLofiSneakyGrowl();

        // Spawn pos achter player
        Vector3 spawnPos = playerController.transform.position - playerCamera.forward * dogSpawnDistance;
        Vector3 rayOrigin = spawnPos + Vector3.up * 5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
            spawnPos.y = hit.point.y + dogSpawnHeightOffset;
        else
            spawnPos.y = playerController.transform.position.y + dogSpawnHeightOffset;

        dog.transform.position = spawnPos;
        dog.transform.LookAt(playerController.transform.position);

        // --- 2. WAIT until player naturally kijkt ---
        while (!PlayerLookingAtDog())
            yield return null;

        // --- Freeze controls ---
        playerController.canMove = false;
        playerController.canLook = false;

        // --- Smooth camera rotation to dog ---
        if (automaticCameraFocus && dogLookTarget != null)
        {
            float elapsed = 0f;
            while (elapsed < dogFocusDuration)
            {
                // … camera rotation logic …
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap exactly
            playerCamera.LookAt(dogLookTarget);

            // Activate extra objects
            objectToActivate?.SetActive(true);
            objectToActivate2?.SetActive(true);

            // 🔥 Trigger Scary Face animation
            dogAnimator?.SetTrigger("ScaryFace");
            
            // Stop LofiSneakyGrowl
            GrowlManager.Instance?.StopLofiSneakyGrowl();

            // Start HeavyBreathing loop + ChaseSiren one-shot
            GrowlManager.Instance?.PlayHeavyBreathing();
            GrowlManager.Instance?.PlayChaseSirenCue();

        }

        // Hold camera frozen
        yield return new WaitForSeconds(lookFreezeDuration);

        // Activate third object AFTER freeze
        objectToActivateAfterFreeze?.SetActive(true);

        // --- 3. Resume player control ---
        playerController.canMove = true;
        playerController.canLook = true;
        playerController.currentState = FirstPersonRabbitController.MovementState.Rabbit;

        // Stop HeavyBreathing loop
        GrowlManager.Instance?.StopHeavyBreathing();

        // Start eerie ambience + Animals Reverb one-shot
        AmbienceManager.Instance?.StartEerieLoop();
        GrowlManager.Instance?.PlayAnimalsReverb();

        // --- 4. Give dog a head start ---
        yield return new WaitForSeconds(headStartDuration);

        // 🔥 Trigger Run animation
        dogAnimator?.SetTrigger("Run");

        // 🔔 AUDIO: Wolf starting chase roar
        GrowlManager.Instance?.PlayWolfStartingChaseRoar();

        // Start chasing player
        dog.GetComponent<DogController>()?.StartChase(playerController.transform);
    }

    private bool PlayerLookingAtDog()
    {
        Vector3 cameraForward = playerCamera.forward;
        Vector3 directionToDog = (dog.transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(cameraForward, directionToDog);
        return dot >= lookAtDogThreshold;
    }
}
