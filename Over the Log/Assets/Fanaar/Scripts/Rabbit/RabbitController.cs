using UnityEngine;

public class RabbitController : MonoBehaviour
{
    [Header("Circle Settings")]
    public Transform danceSpot;        // Midden van de cirkel
    public float radius = 1.5f;        // Afstand van het midden
    public float rotationSpeed = 30f;  // Rotatiesnelheid van de cirkel

    [Header("Movement Settings")]
    public float moveSpeed = 2f;       // Snelheid tot cirkel
    public float orbitSmoothSpeed = 2f; // Hoe snel hij aansluit en meedraait

    [Header("Circle Index")]
    public int circleIndex = 0;        // Slot van dit konijn
    public int totalRabbits = 1;       // Totaal aantal konijnen in cirkel

    [Header("Run Away Settings")]
    public bool isRunningAway = false;
    public Vector3 runDirection = Vector3.forward; // Richting waarin ze wegrennen
    public float runSpeed = 5f;                     // Snelheid van wegrennen
    public float spreadAmount = 1f;                // Spreiding tussen konijnen

    private Vector3 targetPosition;
    private bool isActivated = false;
    private bool isAtDanceSpot = false;

    public bool IsAtDanceSpot => isAtDanceSpot;

    void OnEnable()
    {
        if (gameObject.activeInHierarchy)
            OnActivated();
    }

    void Update()
    {
        // --- RUN AWAY MODE ---
        if (isRunningAway)
        {
            // Bereken target met spreiding
            Vector3 offset = Vector3.right * ((circleIndex - totalRabbits / 2f) * spreadAmount);
            Vector3 runTarget = transform.position + runDirection.normalized * 10f + offset; // 10f = afstand

            // Beweeg naar target
            transform.position = Vector3.MoveTowards(transform.position, runTarget, runSpeed * Time.deltaTime);

            // Kijk in rijrichting
            if ((runTarget - transform.position).magnitude > 0.1f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(runDirection), Time.deltaTime * 5f);

            return; // skip rest van Update
        }

        if (!isActivated) return;

        // --- MOVE TO CIRCLE ---
        if (!isAtDanceSpot)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.LookAt(danceSpot);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isAtDanceSpot = true;
                Debug.Log(name + " is aangekomen op zijn plek in de cirkel!");
            }
        }
        else
        {
            // --- SMOOTH ORBIT ---
            float angle = 360f / totalRabbits * circleIndex + Time.time * rotationSpeed;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 desiredPos = danceSpot.position + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * orbitSmoothSpeed);

            // --- TANGENT LOOK ---
            Vector3 radiusDir = (transform.position - danceSpot.position).normalized; // van centrum naar konijn
            Vector3 tangentDir = -Vector3.Cross(Vector3.up, radiusDir); // tangent richting (clockwise)
            Quaternion targetRotation = Quaternion.LookRotation(tangentDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void OnActivated()
    {
        if (isActivated) return;
        isActivated = true;

        float angle = 360f / totalRabbits * circleIndex;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        targetPosition = danceSpot.position + offset;

        Debug.Log(name + " OnActivated! Doelpositie: " + targetPosition);
    }

    public void RunAway(Vector3 direction)
    {
        isRunningAway = true;
        runDirection = direction.normalized;
        Debug.Log(name + " gaat wegrennen!");
    }
}
