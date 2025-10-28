using UnityEngine;

public class PlayerTriggerLock : MonoBehaviour
{
    [Header("Player Settings")]
    public FirstPersonController playerController;  // sleep hier je player in
    public Vector3 lockedRotationEuler;             // gewenste vaste kijkrichting

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Zet rotatie op slot
        playerController.LockRotation(lockedRotationEuler);

        // Sprinten mag vanaf nu
        playerController.canSprint = true;
    }
}
