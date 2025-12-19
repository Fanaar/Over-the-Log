using UnityEngine;

public class EerieEndingTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // voorkomt dat hij meerdere keren afgaat

        if (other.CompareTag("Player"))
        {
            triggered = true;

            // 🔔 Start de ending ambience
            AmbienceManager.Instance?.PlayEerieEnding();
        }
    }
}
