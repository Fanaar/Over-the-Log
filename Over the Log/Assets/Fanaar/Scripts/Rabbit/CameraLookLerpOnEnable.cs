using UnityEngine;
using System.Collections;

public class CameraLookLerpOnEnable : MonoBehaviour
{
    [Header("References")]
    public FirstPersonRabbitController playerController;
    public Transform lookTarget;

    [Header("Timings")]
    public float lerpToTargetTime = 1f;
    public float holdTime = 0.5f;
    public float lerpBackTime = 1f;

    private bool isLerping;

    private void OnEnable()
    {
        if (!isLerping && playerController != null && lookTarget != null)
        {
            StartCoroutine(LerpRoutine());
        }
    }

    private IEnumerator LerpRoutine()
    {
        isLerping = true;

        // 🔒 Lock input
        playerController.canLook = false;
        playerController.canMove = false;

        // 📸 Start rotatie opslaan (WORLD rotation)
        Quaternion startRotation = playerController.playerCamera.rotation;

        // 🎯 Target rotatie
        Quaternion targetRotation = Quaternion.LookRotation(
            lookTarget.position - playerController.playerCamera.position
        );

        // ➡️ Lerp naar target
        yield return StartCoroutine(LerpRotation(startRotation, targetRotation, lerpToTargetTime));

        // ⏸ Hold
        yield return new WaitForSeconds(holdTime);

        // ⬅️ Lerp terug
        yield return StartCoroutine(LerpRotation(targetRotation, startRotation, lerpBackTime));

        // 🔓 Unlock input
        playerController.canLook = true;
        playerController.canMove = true;

        isLerping = false;
    }

    private IEnumerator LerpRotation(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Quaternion current = Quaternion.Slerp(from, to, t);

            // ⭐ Cruciaal: force via controller
            playerController.ForceLookRotation(current);

            yield return null;
        }

        playerController.ForceLookRotation(to);
    }
}
