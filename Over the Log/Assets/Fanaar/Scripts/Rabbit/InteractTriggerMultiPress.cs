using UnityEngine;

public class InteractTriggerMultiPress : MonoBehaviour
{
    [Header("Interaction")]
    public int requiredPresses = 10;
    public KeyCode interactKey = KeyCode.E;

    [Header("References")]
    public GameObject objectToDeactivate;
    public ParticleSystem pressParticle;

    private int currentPresses = 0;
    private bool playerInside = false;
    private bool completed = false;

    private void Update()
    {
        if (!playerInside || completed) return;

        if (Input.GetKeyDown(interactKey))
        {
            currentPresses++;

            Debug.Log($"[InteractTriggerMultiPress] E pressed ({currentPresses}/{requiredPresses})");

            if (pressParticle != null)
            {
                pressParticle.Play();
                Debug.Log("[InteractTriggerMultiPress] Particle triggered");
            }
            else
            {
                Debug.LogWarning("[InteractTriggerMultiPress] No particle system assigned");
            }

            if (currentPresses >= requiredPresses)
            {
                CompleteInteraction();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            Debug.Log("[InteractTriggerMultiPress] Player entered trigger");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("[InteractTriggerMultiPress] Player exited trigger");
        }
    }

    private void CompleteInteraction()
    {
        completed = true;

        Debug.Log("[InteractTriggerMultiPress] Interaction completed");

        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
            Debug.Log("[InteractTriggerMultiPress] Object deactivated");
        }
        else
        {
            Debug.LogWarning("[InteractTriggerMultiPress] No object assigned to deactivate");
        }
    }
}
