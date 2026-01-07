using UnityEngine;

public class DogActivateTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject dogObject;      // Sleep hier het Dog GameObject in
    public string playerTag = "Player";

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag(playerTag))
        {
            // Activeer hond object
            if (dogObject != null)
            {
                dogObject.SetActive(true);
                Debug.Log("Dog GameObject geactiveerd via trigger!");
            }

            // Zet stressManager bool
            if (StressManager.Instance != null)
            {
                StressManager.Instance.dogIsActive = true;
                Debug.Log("StressManager.dogIsActive = true");
            }

            activated = true;
        }
    }
}
