using UnityEngine;

public class RunTrigger : MonoBehaviour
{
    public RabbitDanceManager manager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.PlayerEnteredTrigger();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.PlayerExitedTrigger();
        }
    }
}
