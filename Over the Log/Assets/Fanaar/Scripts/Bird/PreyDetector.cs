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
            Debug.Log("Prey in range!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Prey"))
        {
            preyInRange = false;
            currentPrey = null;
            Debug.Log("Prey out of range!");
        }
    }
}
