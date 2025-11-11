using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonRabbitController : MonoBehaviour
{
    public enum MovementState { Human, Rabbit }
    public MovementState currentState = MovementState.Human;
    private Vector3 originalScale;
    public float rabbitScale = 0.6f;


    [Header("Human Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.5f;

    [Header("Rabbit Settings")]
    public float rabbitSpeed = 14f;
    public float rabbitAcceleration = 30f;
    public float rabbitJumpHeight = 3f;
    public float airControl = 0.6f; // control while in air

    [Header("Shared Settings")]
    public float gravity = -9.81f;
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float cameraClampAngle = 85f;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation;
    private Vector3 currentMoveVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originalScale = transform.localScale;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SwitchState();

        HandleMouseLook();
        HandleMovement();
    }


    // --------------------------
    // LOOK
    // --------------------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -cameraClampAngle, cameraClampAngle);

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.localEulerAngles = Vector3.right * verticalRotation;
    }

    // --------------------------
    // MOVEMENT (Human / Rabbit switching)
    // --------------------------
    void HandleMovement()
    {
        switch (currentState)
        {
            case MovementState.Human:
                HumanMovement();
                break;
            case MovementState.Rabbit:
                RabbitMovement();
                break;
        }
    }

    void HumanMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float speed = (Input.GetKey(KeyCode.LeftShift)) ? sprintSpeed : walkSpeed;
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;

        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void RabbitMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 targetMove = (transform.right * moveX + transform.forward * moveZ).normalized;

        float accel = controller.isGrounded ? rabbitAcceleration : rabbitAcceleration * airControl;

        currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, targetMove * rabbitSpeed, accel * Time.deltaTime);
        controller.Move(currentMoveVelocity * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void SwitchState()
    {
        currentState = currentState == MovementState.Human ? MovementState.Rabbit : MovementState.Human;
        Debug.Log("🐇 Movement state switched to: " + currentState);

        if (currentState == MovementState.Rabbit)
            transform.localScale = originalScale * rabbitScale;
        else
            transform.localScale = originalScale;
    }

}
