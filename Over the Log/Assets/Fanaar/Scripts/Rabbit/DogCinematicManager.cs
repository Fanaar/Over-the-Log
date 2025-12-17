using UnityEngine;
using System.Collections;
using FMODUnity;

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

    [Header("Cinematic Extra Objects")]
    public GameObject objectToActivate;
    public GameObject objectToActivate2;
    public GameObject objectToActivateAfterFreeze;

    [Header("Dog Animator")]
    public Animator dogAnimator;

    [Header("Audio (Inspector Drag & Drop)")]
    public StudioEventEmitter chase3DAudioEmitter;      // 3D growl for dog while chasing
    public StudioEventEmitter lofiSneakyGrowl;          // starts when dog spawns
    public StudioEventEmitter heavyBreathingLoop;       // starts when camera locks
    public StudioEventEmitter chaseSirenCue;            // one-shot when camera locks
    public StudioEventEmitter animalsReverbCue;         // one-shot when controls unlock
    public StudioEventEmitter wolfChaseRoarCue;         // one-shot when chase starts

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

        // AUDIO: Fade out normal ambience & start sneaky growl
        AmbienceManager.Instance?.FadeOutIntro();
        lofiSneakyGrowl?.Play();

        // Spawn position achter player
        Vector3 spawnPos = playerController.transform.position - playerCamera.forward * dogSpawnDistance;
        Vector3 rayOrigin = spawnPos + Vector3.up * 5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
            spawnPos.y = hit.point.y + dogSpawnHeightOffset;
        else
            spawnPos.y = playerController.transform.position.y + dogSpawnHeightOffset;

        dog.transform.position = spawnPos;
        dog.transform.LookAt(playerController.transform.position);

        // --- 2. Wait until player naturally kijkt ---
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
                Vector3 dir = (dogLookTarget.position - playerCamera.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(dir);
                playerCamera.rotation = Quaternion.Slerp(playerCamera.rotation, targetRot, Time.deltaTime * cameraLerpSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            playerCamera.LookAt(dogLookTarget);

            // Activate extra objects
            objectToActivate?.SetActive(true);
            objectToActivate2?.SetActive(true);

            // Trigger Scary Face animation
            dogAnimator?.SetTrigger("ScaryFace");

            // Stop sneaky growl, start heavy breathing & siren
            lofiSneakyGrowl?.Stop();
            heavyBreathingLoop?.Play();
            chaseSirenCue?.Play();
        }

        // Hold camera frozen
        yield return new WaitForSeconds(lookFreezeDuration);

        // Activate third object after freeze
        objectToActivateAfterFreeze?.SetActive(true);

        // --- Resume player control ---
        playerController.canMove = true;
        playerController.canLook = true;
        playerController.currentState = FirstPersonRabbitController.MovementState.Rabbit;

        // Stop heavy breathing loop
        heavyBreathingLoop?.Stop();

        // Start eerie ambience + animals reverb one-shot
        AmbienceManager.Instance?.StartEerieLoop();
        animalsReverbCue?.Play();

        // --- 4. Give dog a head start ---
        yield return new WaitForSeconds(headStartDuration);

        // Trigger Run animation
        dogAnimator?.SetTrigger("Run");

        // Wolf chase roar
        wolfChaseRoarCue?.Play();

        // Start chasing player
        dog.GetComponent<DogController>()?.StartChase(playerController.transform);

        // Start 3D chase audio
        chase3DAudioEmitter?.Play();
    }

    private bool PlayerLookingAtDog()
    {
        Vector3 cameraForward = playerCamera.forward;
        Vector3 directionToDog = (dog.transform.position - playerCamera.transform.position).normalized;
        float dot = Vector3.Dot(cameraForward, directionToDog);
        return dot >= lookAtDogThreshold;
    }
}
