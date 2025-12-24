using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTracker : MonoBehaviour
{
    public int requiredTriggers = 3;
    private HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    [Header("Smooth Look Settings")]
    public Transform playerCamera;
    public Transform lookTarget; // het punt waar de speler naar moet kijken
    public float lookSpeed = 2f;

    [Header("Activate Object")]
    public GameObject objectToActivate;

    [Header("Camera Lock")]
    public MonoBehaviour cameraController; // bv. MouseLook script

    public void RegisterTrigger(Collider trigger)
    {
        if (triggeredColliders.Contains(trigger))
            return;

        triggeredColliders.Add(trigger);

        Debug.Log($"Trigger count: {triggeredColliders.Count}/{requiredTriggers}");

        if (triggeredColliders.Count >= requiredTriggers)
        {
            OnRequiredTriggersReached();
        }
    }

    private void OnRequiredTriggersReached()
    {
        Debug.Log("🎉 Genoeg triggers geraakt!");

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (playerCamera != null && lookTarget != null)
            StartCoroutine(SmoothLookAndLock());
    }

    private IEnumerator SmoothLookAndLock()
    {
        // Eerst tijdelijk input uitzetten
        if (cameraController != null)
            cameraController.enabled = false;

        float maxDegreesPerSecond = 90f; // snelheid van draaien, pas aan naar wens

        while (true)
        {
            Vector3 direction = (lookTarget.position - playerCamera.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // RotateTowards met maxDegreesPerSecond
            playerCamera.rotation = Quaternion.RotateTowards(playerCamera.rotation, targetRotation, maxDegreesPerSecond * Time.deltaTime);

            // Check of we dicht genoeg zijn bij target
            if (Quaternion.Angle(playerCamera.rotation, targetRotation) < 0.1f)
            {
                playerCamera.rotation = targetRotation; // fix kleine afwijking
                break;
            }

            yield return null;
        }

        Debug.Log("Camera is nu gelocked op target.");
    }

}
