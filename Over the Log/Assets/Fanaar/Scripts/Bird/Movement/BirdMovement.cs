using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BirdMovement : MonoBehaviour
{
    [Header("Flight Settings")]
    public float flightSpeed = 10f;
    public float ascentSpeed = 5f;
    public float minFlightHeight = 1f;
    public float maxFlightHeight = 120f;

    [Header("Bird Tilt Settings")]
    public float maxTiltAngle = 20f;
    public float tiltSpeed = 5f;
    private float currentTiltZ = 0f;

    [Header("Auto Flight Target")]
    public Transform flightTarget; // assign in inspector

    private CharacterController controller;
    private Vector3 velocity;

    void Awake() => controller = GetComponent<CharacterController>();

    public void HandleMovement(PlayerInputHandler input)
    {
        if (flightTarget == null)
        {
            Debug.LogWarning("Flight target not assigned!");
            return;
        }

        // --- CALCULATE DIRECTION TOWARD TARGET ---
        Vector3 targetDir = (flightTarget.position - transform.position).normalized;

        // Forward movement is automatic toward target
        Vector3 forwardVelocity = targetDir * flightSpeed;

        // Strafe tilt using input (for visuals)
        float moveX = input.MoveX;
        Vector3 strafeVelocity = transform.right * moveX * flightSpeed * 0.5f; // optional influence

        velocity.x = forwardVelocity.x + strafeVelocity.x;
        velocity.z = forwardVelocity.z + strafeVelocity.z;

        // --- ASCEND/DESCEND ---
        float verticalInput = input.MoveZ;
        if (transform.position.y < minFlightHeight)
        {
            velocity.y = ascentSpeed;
        }
        else if (verticalInput > 0f && transform.position.y < maxFlightHeight)
        {
            velocity.y = verticalInput * ascentSpeed;
        }
        else if (verticalInput < 0f && transform.position.y > minFlightHeight)
        {
            velocity.y = verticalInput * ascentSpeed;
        }
        else
        {
            velocity.y = 0f;
        }

        // --- TILT ---
        float targetTilt = -moveX * maxTiltAngle;
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSpeed);

        Vector3 localAngles = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(localAngles.x, localAngles.y, currentTiltZ);

        // --- MOVE ---
        controller.Move(velocity * Time.deltaTime);

        // Optional: rotate bird to face target smoothly
        Vector3 lookDir = new Vector3(targetDir.x, 0f, targetDir.z); // keep bird level
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 2f);
    }
}
