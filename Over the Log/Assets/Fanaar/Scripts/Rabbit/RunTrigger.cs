using UnityEngine;

public class RunTrigger : MonoBehaviour
{
    public RabbitDanceManager manager;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter: " + other.name); // zie wie erin komt
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger!");
            manager.PlayerEnteredTrigger();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit: " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited trigger!");
            manager.PlayerExitedTrigger();
        }
    }
}
