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
    public float lookSpeed = 2f;

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
    public AudioSource audioSource;       // AudioSource voor de trigger
    public AudioClip activationClip;      // Clip die afspeelt bij objectToActivate

    // 🔹 Flags
    private bool triggersComplete = false;
    private bool audioSequenceFinished = false;
    private bool activationAudioPlayed = false;  // ✅ check dat audio maar één keer speelt

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

        if (triggeredColliders.Count >= requiredTriggers)
        {
            triggersComplete = true;
            TryActivateFinalEffects();
        }
    }

    private void OnAudioSequenceFinished()
    {
        audioSequenceFinished = true;
        TryActivateFinalEffects();
    }

    private void TryActivateFinalEffects()
    {
        // ✅ Beide voorwaarden moeten waar zijn
        if (!triggersComplete || !audioSequenceFinished)
            return;

        Debug.Log("⚡ Triggers + Audio klaar — bliksem en objecten activeren!");

        // Object activeren
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);

            // Speel audio maar één keer
            if (!activationAudioPlayed && audioSource != null && activationClip != null)
            {
                audioSource.PlayOneShot(activationClip);
                activationAudioPlayed = true;
            }
        }

        // Trigger bliksem
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
        GameObject[] collectTriggers = GameObject.FindGameObjectsWithTag(collectTriggerTag);

        foreach (GameObject obj in collectTriggers)
        {
            obj.SetActive(false);
        }

        Debug.Log($"🧹 {collectTriggers.Length} CollectTriggers gedeactiveerd.");
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

            playerCamera.rotation = Quaternion.RotateTowards(playerCamera.rotation, targetRotation, maxDegreesPerSecond * Time.deltaTime);

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
