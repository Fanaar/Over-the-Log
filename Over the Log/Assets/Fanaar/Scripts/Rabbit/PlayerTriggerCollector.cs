using UnityEngine;

public class PlayerTriggerCollector : MonoBehaviour
{
    public TriggerTracker tracker;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag("CollectTrigger"))
            return;

        // Eerst log
        Debug.Log($"Trigger geraakt door speler: {hit.collider.name}");

        // Daarna tellen
        tracker.RegisterTrigger(hit.collider);

        // Dan pas deactiveer
        hit.collider.gameObject.SetActive(false);
    }

}
