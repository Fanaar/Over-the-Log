using UnityEngine;

public class PreyNearMissDetector : MonoBehaviour
{
    [HideInInspector] public bool preyNear = false;
    [HideInInspector] public GameObject currentNearMissPrey = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Prey"))
        {
            preyNear = true;
            currentNearMissPrey = other.gameObject;
            Debug.Log("📍 Prey entered NEAR-MISS range: " + other.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Prey"))
        {
            preyNear = false;
            currentNearMissPrey = null;
            Debug.Log("❌ Prey left NEAR-MISS range: " + other.name);
        }
    }

    void Update()
    {
        if (preyNear && currentNearMissPrey != null) ;
           // Debug.Log("✅ Prey still in NEAR-MISS range: " + currentNearMissPrey.name);
    }
}