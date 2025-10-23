using UnityEngine;

public class BirdTrigger : MonoBehaviour
{
    public BirdController bird; // Sleep hier de vogel in de inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bird.GoToNextWaypoint();
        }
    }
}
