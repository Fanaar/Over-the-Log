using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    public DogBehaviour dog;
    public FirstPersonRabbitController player;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        dog.StartEncounter(other.transform);
        player.canMove = false;

        gameObject.SetActive(false);
    }
}
