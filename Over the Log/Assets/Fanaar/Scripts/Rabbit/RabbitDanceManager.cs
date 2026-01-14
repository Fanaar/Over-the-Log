using UnityEngine;
using System.Collections;

public class RabbitDanceManager : MonoBehaviour
{
    [Header("Rabbits")]
    public RabbitController[] rabbits;
    public float rotationSpeed = 30f;

    [Header("Rounds Settings")]
    public int roundsBeforeRunAway = 1;

    [Header("Run Away Settings")]
    public Transform runStart;
    public Transform runEnd;
    public bool useRandomBetweenTransforms = true;

    [Header("Dog Settings")]
    public GameObject dog;
    public Transform player;
    public Transform dogSpawnPoint; // instead of dogSpawnDistance

    [Header("Player Look Settings")]
    public Transform dogLookTarget; // Inspector target
    [Range(-1f, 1f)]
    public float lookDotThreshold = 0.5f; // smaller = wider cone

    [Header("Debug Gizmo")]
    public bool showLookCone = true;
    public float coneLength = 5f;

    [Header("Dog Cinematic")]
    public DogCinematicManager dogCinematicManager; // assign in inspector

    [Header("Run Condition Trigger")]
    public Collider playerRunTrigger; // sleep hier de trigger in inspector
    private bool playerInTrigger = false;

    [Header("Managers")]
    public RabbitManager rabbitManager;

    [Header("Camera Lerp Settings")]
    public Transform cameraLookTarget;   // waar de camera naartoe kijkt
    public float cameraLerpSpeed = 2f;   // hoe snel de lerp gaat
    public float cameraHoldTime = 0.5f;  // hoe lang hij blijft hangen (optioneel)

    private bool isCameraLerping = false;

    [SerializeField] private bool allReady = false;  // blijft in inspector zichtbaar
    public bool AllReady => allReady;               // read-only voor andere scripts

    private float currentAngle = 0f;
    private int completedRotations = 0;

    private bool hasRunAway = false;

    void Update()
    {
        // Check if all rabbits are in position
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

        // Rotate circle
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

        // Check if rabbits can start running
        if (!hasRunAway && completedRotations >= roundsBeforeRunAway && playerInTrigger)
        {
            hasRunAway = true; // 🔒 lock meteen

            var controller = player.GetComponent<FirstPersonRabbitController>();
            if (controller != null)
            {
                controller.canMove = false;
                controller.canLook = false; // tijdelijk uit
            }

            StartRunAway();

        }
    }

    private void StartRunAway()
    {
        // 🔴 Ontkoppel konijnen van hun container
        foreach (var rabbit in rabbits)
            rabbit.transform.SetParent(null, true);

        rabbitManager?.StopCircleDance();

        if (runStart == null || runEnd == null)
        {
            Debug.LogError("RunStart of RunEnd ontbreekt!");
            return;
        }

        Vector3 runVector = runEnd.position - runStart.position;
        float runDistance = runVector.magnitude;
        Vector3 runDir = runVector.normalized;
        Vector3 side = Vector3.Cross(Vector3.up, runDir);

        foreach (var rabbit in rabbits)
        {
            float forward = Random.Range(0.6f, 1f) * runDistance;
            float sideways = Random.Range(-1.5f, 1.5f);

            Vector3 target =
                runStart.position +
                runDir * forward +
                side * sideways;

            Debug.DrawLine(rabbit.transform.position, target, Color.green, 3f);

            rabbit.RunAwayTo(target);
        }

        Debug.Log("🐇 Konijnen rennen weg!");

        if (cameraLookTarget != null && !isCameraLerping)
        {
            StartCoroutine(LerpCameraToTarget());
        }
    }


    private void SpawnDogClean()
    {
        if (dog == null || dogSpawnPoint == null)
            return;

        // 1. Set position & rotation to spawn point
        dog.transform.position = dogSpawnPoint.position;
        dog.transform.rotation = dogSpawnPoint.rotation;

        // 2. Activate the dog
        dog.SetActive(true);

        // 3. Force ground snap & fix X rotation immediately
        DogController ctrl = dog.GetComponent<DogController>();
        if (ctrl != null)
        {
            ctrl.ForceGroundSnap(); // snaps dog to terrain immediately
        }
    }

    private bool PlayerLookingAtRunDirection()
    {
        if (dogLookTarget == null) return true;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 dirToTarget = (dogLookTarget.position - player.position).normalized;
        float dot = Vector3.Dot(cameraForward, dirToTarget);
        return dot >= lookDotThreshold;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
            playerInTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform == player)
            playerInTrigger = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!showLookCone || player == null || dogLookTarget == null) return;

        Vector3 origin = player.position + Vector3.up; // adjust for eyes
        Vector3 dir = (dogLookTarget.position - origin).normalized;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, dir * coneLength);
    }

    public void PlayerEnteredTrigger()
    {
        playerInTrigger = true;
    }

    public void PlayerExitedTrigger()
    {
        playerInTrigger = false;
    }


    private IEnumerator LerpCameraToTarget()
    {
        isCameraLerping = true;

        Camera cam = Camera.main;
        if (cam == null)
            yield break;

        Transform camTransform = cam.transform;
        Quaternion startRot = camTransform.rotation;

        Vector3 dir = (cameraLookTarget.position - camTransform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraLerpSpeed;
            camTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // Kleine cinematic pauze
        if (cameraHoldTime > 0f)
            yield return new WaitForSeconds(cameraHoldTime);

        var controller = player.GetComponent<FirstPersonRabbitController>();
        if (controller != null)
        {
            controller.ForceLookRotation(camTransform.rotation);
            controller.canLook = true;
            controller.canMove = false;
        }

        isCameraLerping = false;

        SpawnDogClean();

        if (dogCinematicManager != null)
            dogCinematicManager.StartCinematic();
    }

}
