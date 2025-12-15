using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        RabbitRunAwayController rabbit = other.GetComponent<RabbitRunAwayController>();
        if (rabbit != null)
        {
            rabbit.isInPlayerTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        RabbitRunAwayController rabbit = other.GetComponent<RabbitRunAwayController>();
        if (rabbit != null)
        {
            rabbit.isInPlayerTrigger = false;
        }
    }
}
