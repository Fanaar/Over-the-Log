using UnityEngine;

public class RabbitDanceTrigger : MonoBehaviour
{
    [SerializeField] private Collider triggerCollider;
    private bool hasTriggered = false;

    private void Awake()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerCollider.enabled) return;
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("🐇 Dance circle TRIGGERED");
    }

    public void EnableTrigger()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }
}
