using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTracker : MonoBehaviour
{
    public int requiredTriggers = 3;
    private HashSet<Collider> triggeredColliders = new HashSet<Collider>();

    [Header("Smooth Look Settings")]
    public Transform playerCamera;
    public Transform lookTarget;

    [Header("Activate Object")]
    public GameObject objectToActivate;

    [Header("Camera Lock")]
    public MonoBehaviour cameraController;

    [Header("Activate Object 2")]
    public GameObject objectToActivate2;
    public float activationDelay = 1f;

    [Header("Post Processing")]
    public PostProcessingWithDarkBordersAndLightning postProcessing;
    public bool disableLightningAfterDelay = true;
    public float lightningDuration = 2f;

    [Header("Collect Trigger Cleanup")]
    public string collectTriggerTag = "CollectTrigger";

    [Header("Audio on Activate Object")]
    public AudioSource audioSource;
    public AudioClip activationClip;

    [Header("Final Delay")]
    public float finalDelayAfterLastTrigger = 3f;

    [Header("Audio on Last Trigger")]
    public AudioSource lastTriggerAudioSource;
    public AudioClip lastTriggerClip;

    // 🔥 NIEUW
    [Header("Reverse Audio Activation")]
    public GameObject reverseAudioObject;
    public float reverseAudioDelay = 1.5f;

    // 🔹 Flags
    private bool triggersComplete = false;
    private bool audioSequenceFinished = true;
    private bool delayAfterTriggersFinished = false;
    private bool activationAudioPlayed = false;

    private void OnEnable()
    {
        TriggerAudioSequenceGlobal.OnSequenceFinished += OnAudioSequenceFinished;
    }

    private void OnDisable()
    {
        TriggerAudioSequenceGlobal.OnSequenceFinished -= OnAudioSequenceFinished;
    }

    public void RegisterTrigger(Collider trigger)
    {
        if (triggeredColliders.Contains(trigger))
            return;

        triggeredColliders.Add(trigger);

        Debug.Log($"Trigger count: {triggeredColliders.Count}/{requiredTriggers}");

        if (triggeredColliders.Count >= requiredTriggers && !triggersComplete)
        {
            triggersComplete = true;

            if (lastTriggerAudioSource != null && lastTriggerClip != null)
                lastTriggerAudioSource.PlayOneShot(lastTriggerClip);

            StartCoroutine(WaitAfterLastTrigger());

            // 🔥 NIEUW
            if (reverseAudioObject != null)
                StartCoroutine(ActivateReverseAudioWithDelay());
        }
    }

    private IEnumerator WaitAfterLastTrigger()
    {
        yield return new WaitForSeconds(finalDelayAfterLastTrigger);
        delayAfterTriggersFinished = true;
        TryActivateFinalEffects();
    }

    // 🔥 NIEUW
    private IEnumerator ActivateReverseAudioWithDelay()
    {
        yield return new WaitForSeconds(reverseAudioDelay);
        reverseAudioObject.SetActive(true);
        Debug.Log("🔊 Reverse audio geactiveerd");
    }

    private void OnAudioSequenceFinished()
    {
        audioSequenceFinished = true;
        TryActivateFinalEffects();
    }

    private void TryActivateFinalEffects()
    {
        if (!triggersComplete || !audioSequenceFinished || !delayAfterTriggersFinished)
            return;

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);

            if (!activationAudioPlayed && audioSource != null && activationClip != null)
            {
                audioSource.PlayOneShot(activationClip);
                activationAudioPlayed = true;
            }
        }

        if (postProcessing != null)
            postProcessing.TriggerLightning(true);

        if (disableLightningAfterDelay)
            StartCoroutine(DisableLightningAfterSeconds(lightningDuration));

        DeactivateCollectTriggers();

        if (playerCamera != null && lookTarget != null)
            StartCoroutine(SmoothLookAndLock());
    }

    private void DeactivateCollectTriggers()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(collectTriggerTag))
            obj.SetActive(false);
    }

    private IEnumerator DisableLightningAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (postProcessing != null)
            postProcessing.TriggerLightning(false);
    }

    private IEnumerator SmoothLookAndLock()
    {
        if (cameraController != null)
            cameraController.enabled = false;

        float maxDegreesPerSecond = 90f;

        while (true)
        {
            Vector3 direction = (lookTarget.position - playerCamera.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            playerCamera.rotation = Quaternion.RotateTowards(
                playerCamera.rotation,
                targetRotation,
                maxDegreesPerSecond * Time.deltaTime
            );

            if (Quaternion.Angle(playerCamera.rotation, targetRotation) < 0.1f)
            {
                playerCamera.rotation = targetRotation;
                break;
            }

            yield return null;
        }

        if (activationDelay > 0f)
            yield return new WaitForSeconds(activationDelay);

        if (objectToActivate2 != null)
            objectToActivate2.SetActive(true);
    }
}
