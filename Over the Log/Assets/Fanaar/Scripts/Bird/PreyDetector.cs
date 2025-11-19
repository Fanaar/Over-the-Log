using UnityEngine;

public class PreyDetector : MonoBehaviour
{
    [HideInInspector] public bool preyInRange = false;
    [HideInInspector] public GameObject currentPrey = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Prey"))
        {
            preyInRange = true;
            currentPrey = other.gameObject;
            Debug.Log("🎯 Prey entered CATCH range: " + other.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Prey"))
        {
            preyInRange = false;
            currentPrey = null;
            Debug.Log("⚠️ Prey left CATCH range: " + other.name);
        }
    }

    void Update()
    {
        if (preyInRange && currentPrey != null) ;
            //Debug.Log("✅ Prey still in CATCH range: " + currentPrey.name);
    }
}
