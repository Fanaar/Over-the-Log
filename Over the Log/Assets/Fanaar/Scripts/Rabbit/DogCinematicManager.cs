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
    public GameObject dollyObject;
    public GameObject fmodAudioObject;
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
        Debug.Log("StartCinematic CALLED", this);

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

        // Spawn position behind player
        Vector3 spawnPos = playerController.transform.position - playerCamera.forward * dogSpawnDistance;
        Vector3 rayOrigin = spawnPos + Vector3.up * 5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
            spawnPos.y = hit.point.y + dogSpawnHeightOffset;
        else
            spawnPos.y = playerController.transform.position.y + dogSpawnHeightOffset;

        dog.transform.position = spawnPos;
        dog.transform.LookAt(playerController.transform.position);

        // --- 2. Wait until player naturally looks at dog ---
        while (!PlayerLookingAtDog())
            yield return null;

        // --- Freeze player controls ---
        playerController.canMove = false;
        playerController.canLook = false;

        // --- Smooth camera rotation to dog ---
        if (automaticCameraFocus && dogLookTarget != null)
        {
            dollyObject?.SetActive(true);

            if (dogAnimator != null)
                dogAnimator.SetBool("isScaryFace", true);

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

            // Stop sneaky growl, start heavy breathing & siren
            if (lofiSneakyGrowl != null)
                lofiSneakyGrowl.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            heavyBreathingLoop?.Play();
            chaseSirenCue?.Play();
        }

        yield return new WaitForSeconds(lookFreezeDuration);

        objectToActivateAfterFreeze?.SetActive(true);

        // --- Resume player control ---
        playerController.canMove = true;
        playerController.canLook = true;
        playerController.currentState = FirstPersonRabbitController.MovementState.Rabbit;

        // Stop heavy breathing loop
        if (heavyBreathingLoop != null)
            heavyBreathingLoop.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        wolfChaseRoarCue?.Play();

        AmbienceManager.Instance?.StartEerieLoop();
        animalsReverbCue?.Play();

        yield return new WaitForSeconds(headStartDuration);

        dogAnimator?.SetTrigger("Run");

        dog.GetComponent<DogController>()?.StartChase(playerController.transform);

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
