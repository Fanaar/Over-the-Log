using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    // Trigger detecteert alle objecten met deze scripts
    void OnTriggerEnter(Collider other)
    {
        // Konijnen
        RabbitRunAwayController rabbit = other.GetComponent<RabbitRunAwayController>();
        if (rabbit != null)
        {
            rabbit.isInPlayerTrigger = true; // Konijn mag bewegen
        }

        // Hond
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
        {
            dog.isInPlayerTrigger = true; // Hond staat stil in trigger
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Konijnen
        RabbitRunAwayController rabbit = other.GetComponent<RabbitRunAwayController>();
        if (rabbit != null)
        {
            rabbit.isInPlayerTrigger = false; // Konijn stopt met bewegen buiten trigger
        }

        // Hond
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
        {
            dog.isInPlayerTrigger = false; // Hond mag bewegen buiten trigger
        }
    }
}
