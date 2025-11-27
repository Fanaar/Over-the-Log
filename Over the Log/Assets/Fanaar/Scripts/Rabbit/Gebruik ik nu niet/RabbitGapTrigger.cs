using UnityEngine;

public class RabbitGapTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("✅ Perfect timing, player entered the gap!");
            // Trigger success event / next scene / animation
        }
    }
}
