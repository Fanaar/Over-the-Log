using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    [SerializeField] private PlatformDescender platform;  // Drag the platform here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && platform != null)
        {
            platform.StartDescending();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && platform != null)
        {
            platform.StartAscending();
        }
    }
}
