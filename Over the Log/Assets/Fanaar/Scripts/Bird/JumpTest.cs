using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class JumpTest : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 velocity;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jump triggered! velocity.y = " + velocity.y);
        }

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
