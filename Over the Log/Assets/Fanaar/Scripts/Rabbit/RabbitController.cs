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

    private Vector3 targetPosition;
    private bool isActivated = false;
    private bool isAtDanceSpot = false;

    void OnEnable()
    {
        if (gameObject.activeInHierarchy)
            OnActivated();
    }

    void Update()
    {
        if (!isActivated) return;

        // Eerst bewegen naar plek in cirkel
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
            // Bepaal rotatiehoek
            float angle = 360f / totalRabbits * circleIndex + Time.time * rotationSpeed;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 desiredPos = danceSpot.position + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

            // Smooth orbit zonder snapping
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * orbitSmoothSpeed);

            // Smooth richting midden draaien
            Quaternion lookDir = Quaternion.LookRotation(danceSpot.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookDir, Time.deltaTime * 5f);
        }
    }

    public void OnActivated()
    {
        if (isActivated) return;
        isActivated = true;

        // Bepaal eerste cirkelpositie
        float angle = 360f / totalRabbits * circleIndex;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        targetPosition = danceSpot.position + offset;

        Debug.Log(name + " OnActivated! Doelpositie: " + targetPosition);
    }
}
