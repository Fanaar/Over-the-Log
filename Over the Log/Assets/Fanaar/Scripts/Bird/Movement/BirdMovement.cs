using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BirdMovement : MonoBehaviour
{
    public float flightSpeed = 10f;
    public float ascentSpeed = 5f;
    public float minFlightHeight = 1f;
    public float maxFlightHeight = 120f;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake() => controller = GetComponent<CharacterController>();

    public void HandleMovement(PlayerInputHandler input)
    {
        Vector3 forward = transform.forward * flightSpeed;
        Vector3 strafe = transform.right * input.MoveX * flightSpeed;

        velocity.x = forward.x + strafe.x;
        velocity.z = forward.z + strafe.z;

        // Ascend / descend
        if (input.MoveZ != 0f)
            velocity.y = input.MoveZ * ascentSpeed;
        else
            velocity.y = 0f;

        // Clamp height
        if (transform.position.y < minFlightHeight) velocity.y = ascentSpeed;
        if (transform.position.y > maxFlightHeight) velocity.y = 0f;

        controller.Move(velocity * Time.deltaTime);
    }
}
