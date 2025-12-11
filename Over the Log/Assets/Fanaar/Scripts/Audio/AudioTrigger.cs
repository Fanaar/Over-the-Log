using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioTrigger : MonoBehaviour
{
    [Header("Activation Settings")]
    public GameObject[] objectsToActivate;

    private void Start()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
