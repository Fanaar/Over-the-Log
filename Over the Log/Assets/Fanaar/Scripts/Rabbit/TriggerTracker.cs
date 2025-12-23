using System.Collections.Generic;
using UnityEngine;

public class TriggerTracker : MonoBehaviour
{
    public int requiredTriggers = 3;

    private HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    public void RegisterTrigger(Collider trigger)
    {
        if (triggeredColliders.Contains(trigger))
            return;

        triggeredColliders.Add(trigger);

        Debug.Log($"Trigger count: {triggeredColliders.Count}/{requiredTriggers}");

        if (triggeredColliders.Count >= requiredTriggers)
        {
            OnRequiredTriggersReached();
        }
    }

    private void OnRequiredTriggersReached()
    {
        Debug.Log("🎉 Genoeg triggers geraakt!");
        // deur open / event / cutscene
    }
}
