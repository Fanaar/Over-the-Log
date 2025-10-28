using UnityEngine;

public class PlayerTriggerLock : MonoBehaviour
{
    [Header("Player Settings")]
    public FirstPersonController playerController;  // Drag your player here
    public Vector3 lockedRotationEuler;             // Desired facing direction
    public float rotationLerpSpeed = 3f;            // Adjust smoothness in inspector

    private bool rotationLockActive = false;
    private Quaternion targetRotation;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Enable sprinting
        playerController.canSprint = true;

        // Activate smooth rotation
        rotationLockActive = true;
        targetRotation = Quaternion.Euler(lockedRotationEuler);
    }

    private void Update()
    {
        if (rotationLockActive && playerController != null)
        {
            // Smoothly rotate the player toward the target direction
            playerController.transform.rotation = Quaternion.Slerp(
                playerController.transform.rotation,
                targetRotation,
                Time.deltaTime * rotationLerpSpeed
            );
        }
    }
}
