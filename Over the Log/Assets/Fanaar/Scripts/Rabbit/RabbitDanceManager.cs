using UnityEngine;

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

    [Header("Audio")]
    public RabbitDanceTrigger rabbitDanceTrigger;


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
        if (!hasRunAway && completedRotations >= roundsBeforeRunAway && PlayerLookingAtRunDirection() && playerInTrigger)
        {
            var controller = player.GetComponent<FirstPersonRabbitController>();
            if (controller != null)
            {
                controller.canMove = false;
                controller.canLook = true;
            }

            StartRunAway();

            if (dogCinematicManager != null)
                dogCinematicManager.StartCinematic();
        }
    }

    private void StartRunAway()
    {
        // Stop circle dance audio
        rabbitDanceTrigger?.StopCircleDance();

        SpawnDogClean();  // NEW

        // Make all rabbits run
        foreach (var rabbit in rabbits)
        {
            Vector3 targetDir;

            if (useRandomBetweenTransforms && runStart != null && runEnd != null)
            {
                // Bereken richting van start naar eind
                Vector3 runVector = (runEnd.position - runStart.position).normalized;
                float runDistance = Vector3.Distance(runStart.position, runEnd.position);

                // Kies een random punt langs de lijn van runStart → runEnd
                float randomDistance = Random.Range(0f, runDistance);
                Vector3 targetPos = runStart.position + runVector * randomDistance;

                // Direction van rabbit naar targetPos
                targetDir = (targetPos - rabbit.transform.position).normalized;
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

}
