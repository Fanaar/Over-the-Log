using UnityEngine;

public class RabbitTracker : MonoBehaviour
{
    private bool hasBeenCollected = false;

    private void OnEnable()
    {
        if (!hasBeenCollected)
        {
            hasBeenCollected = true;

            // Check in RabbitManager of dit konijn al geregistreerd is
            if (RabbitManager.Instance != null)
            {
                RabbitManager.Instance.RegisterRabbitActivated();
            }
        }
    }
}
