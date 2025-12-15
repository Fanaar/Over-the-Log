using UnityEngine;

public class DogPlayerTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
            dog.isInPlayerTrigger = true; // hond stil
    }

    void OnTriggerExit(Collider other)
    {
        DogController2 dog = other.GetComponent<DogController2>();
        if (dog != null)
            dog.isInPlayerTrigger = false; // hond beweegt
    }
}
