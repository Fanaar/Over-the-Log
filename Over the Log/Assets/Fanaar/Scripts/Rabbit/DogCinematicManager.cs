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

    [Header("Second Camera Beat")]
    public Transform finalLookTarget;
    public float finalCameraLerpSpeed = 2.5f;
    public float finalLerpMinDuration = 0.4f;


    [Header("Cinematic Extra Objects")]
    public GameObject dollyObject;
    public GameObject fmodAudioObject;
    public GameObject objectToActivateAfterFreeze;
    public GameObject spikesObject;
    public GameObject spikesCollider;

    [Header("Dog Animator")]
    public Animator dogAnimator;

    [Header("Audio (Inspector Drag & Drop)")]
    public StudioEventEmitter chase3DAudioEmitter;      // 3D growl for dog while chasing
    public StudioEventEmitter heavyBreathingLoop;       // starts when camera locks
    public StudioEventEmitter chaseSirenCue;            // one-shot when camera locks
    public StudioEventEmitter animalsReverbCue;         // one-shot when controls unlock
    public StudioEventEmitter wolfChaseRoarCue;         // one-shot when chase starts
    public StudioEventEmitter wolfChaseMusic;         // music when chase starts

    private bool cinematicStarted = false;
    private float originalSprintMultiplier;


    public void StartCinematic()
    {
        Debug.Log("StartCinematic CALLED", this);

        if (cinematicStarted) return;
        cinematicStarted = true;

        originalSprintMultiplier = playerController.sprintMultiplier;

        StartCoroutine(CinematicSequence());
    }

    private IEnumerator CinematicSequence()
    {
        // --- 1. Spawn dog ---
        dog.SetActive(true);

        // AUDIO: Fade out normal ambience & start sneaky growl
        AmbienceManager.Instance?.FadeOutIntro();
        heavyBreathingLoop?.Play();

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
            yield return StartCoroutine(LerpCameraToTarget(dogLookTarget,cameraLerpSpeed,dogFocusDuration));

            chaseSirenCue?.Play();
        }

        yield return new WaitForSeconds(lookFreezeDuration);

        objectToActivateAfterFreeze?.SetActive(true);

        // 🎥 SECOND CAMERA BEAT (pre-unlock)
        if (finalLookTarget != null)
        {
            yield return StartCoroutine(LerpCameraToTarget(finalLookTarget,finalCameraLerpSpeed,finalLerpMinDuration));
        }

        // 🔒 Sync player look state met laatste camera rotatie
        playerController.ForceLookRotation(playerCamera.rotation);

        // --- Resume player control ---
        playerController.canMove = true;
        playerController.canLook = true;
        playerController.sprintMultiplier = 3.5f;

        spikesObject.SetActive(true);
        spikesCollider.SetActive(true);

        // Stop heavy breathing loop
        if (heavyBreathingLoop != null)
            heavyBreathingLoop.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        wolfChaseRoarCue?.Play();
        wolfChaseMusic.Play();

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

    private IEnumerator LerpCameraToTarget(
    Transform lookTarget,
    float lerpSpeed,
    float minDuration = 0f
)
    {
        if (lookTarget == null)
            yield break;

        float elapsed = 0f;

        while (true)
        {
            Vector3 dir = (lookTarget.position - playerCamera.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dir);

            playerCamera.rotation = Quaternion.Slerp(
                playerCamera.rotation,
                targetRot,
                Time.deltaTime * lerpSpeed
            );

            elapsed += Time.deltaTime;

            // Stop wanneer:
            // - minimale tijd voorbij is
            // - EN camera bijna correct kijkt
            if (elapsed >= minDuration &&
                Quaternion.Angle(playerCamera.rotation, targetRot) < 0.5f)
                break;

            yield return null;
        }

        // Force exact eindpunt
        playerCamera.LookAt(lookTarget);
    }

}
