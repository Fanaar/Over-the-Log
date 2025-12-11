using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ActivateOnceOnTrigger : MonoBehaviour
{
    [Header("Activation Settings")]
    public GameObject[] objectsToActivate;
    public bool playOnce = true;

    private bool hasActivated = false;

    private void Start()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasActivated && playOnce) return;

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        hasActivated = true;
    }
}
