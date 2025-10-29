using UnityEngine;

public class ControlTrigger : MonoBehaviour
{
    public int controlIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Fade in
        ControlManager.Instance.ShowControl(controlIndex);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Fade out
        ControlManager.Instance.HideControl(controlIndex);

        // Zet de trigger uit zodat het niet opnieuw afgaat
        gameObject.SetActive(false);
    }
}
