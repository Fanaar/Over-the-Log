using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HumanMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float flapBoost = 2f;
    public float flapCooldown = 0.3f;

    private CharacterController controller;
    private Vector3 velocity;
    private float lastFlapTime;
    public int flapProgress = 0;

    public bool flightUnlocked = false;

    void Awake() => controller = GetComponent<CharacterController>();

    public void HandleMovement(PlayerInputHandler input)
    {
        Vector3 move = transform.right * input.MoveX + transform.forward * input.MoveZ;
        float speed = input.SprintHeld ? sprintSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleJumpFlap(PlayerInputHandler input)
    {
        if (!controller.isGrounded) return;

        if (input.JumpPressed)
        {
            // Jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Flap progress
            if (Time.time - lastFlapTime > flapCooldown)
            {
                lastFlapTime = Time.time;
                flapProgress++;
                velocity.y = flapBoost;

                if (flapProgress >= 3)
                    flightUnlocked = true;
            }
        }
    }
}
