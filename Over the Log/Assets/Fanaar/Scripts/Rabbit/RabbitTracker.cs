using UnityEngine;

public class RabbitTracker : MonoBehaviour
{
    private bool hasBeenCollected = false;

    private void OnEnable()
    {
        // Dit wordt geroepen elke keer als het konijn active wordt
        if (!hasBeenCollected)
        {
            hasBeenCollected = true;
            RabbitManager.Instance.RegisterRabbitActivated();
        }
    }
}
