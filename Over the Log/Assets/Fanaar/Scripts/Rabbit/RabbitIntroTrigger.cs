using UnityEngine;

public class RabbitIntroTrigger : MonoBehaviour
{
    public RabbitIntroController rabbitController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        rabbitController.StartIntro();
        gameObject.SetActive(false); // Trigger eenmalig
    }
}
