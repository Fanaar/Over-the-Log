using UnityEngine;

public class RabbitController : MonoBehaviour
{
    public Transform danceSpot;    // Midden van de cirkel
    public float radius = 1.5f;    // Hoe ver van het midden
    public float moveSpeed = 2f;

    public int circleIndex = 0;     // Welk nummer in de cirkel
    public int totalRabbits = 1;   // Totaal aantal konijnen dat de cirkel vormt

    private Vector3 targetPosition;
    private bool isActivated = false;
    private bool isAtDanceSpot = false;

    void OnEnable()
    {
        if (gameObject.activeInHierarchy)
        {
            OnActivated();
        }
    }

    void Update()
    {
        if (!isActivated) return;

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
    }

    public void OnActivated()
    {
        if (isActivated) return;
        isActivated = true;

        // Bereken cirkelpositie
        float angle = 360f / totalRabbits * circleIndex;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        targetPosition = danceSpot.position + offset;

        Debug.Log(name + " OnActivated! Doelpositie: " + targetPosition);
    }
}
