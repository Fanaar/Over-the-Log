using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonRabbitController : MonoBehaviour
{
    public enum MovementState { Human, Rabbit }
    public MovementState currentState = MovementState.Human;

    [Header("Human Settings")]
    public float walkSpeed = 5f;
    public float jumpHeight = 1.5f;

    [Header("Rabbit Settings")]
    public float rabbitSpeed = 14f;
    public float rabbitAcceleration = 30f;
    public float rabbitJumpHeight = 3f;
    public float airControl = 0.6f;

    [Header("Shared Settings")]
    public float gravity = -9.81f;
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float cameraClampAngle = 85f;

    [Header("Ground Check Settings")]
    public Transform groundCheckPoint; // plaats net onder de voeten
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [HideInInspector] public bool canLook = true; // freeze camera when false

    [HideInInspector] public bool canMove = true;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private float verticalRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Always allow mouse look
        HandleMouseLook();

        // Only allow movement if canMove is true
        if (canMove)
            HandleMovement();

        // Allow state switch anytime
        if (Input.GetKeyDown(KeyCode.R))
            SwitchState();
    }


    // --------------------------
    // MOUSE LOOK
    // --------------------------
    void HandleMouseLook()
    {
        if (!canLook) return; // skip looking if frozen

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -cameraClampAngle, cameraClampAngle);

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.localEulerAngles = Vector3.right * verticalRotation;
    }


    // --------------------------
    // MOVEMENT
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

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    void HumanMovement()
    {
        Vector3 move = (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical")).normalized;
        controller.Move(move * walkSpeed * Time.deltaTime);

        bool grounded = IsGrounded();
        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && grounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void RabbitMovement()
    {
        Vector3 targetMove = (transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical")).normalized;
        float accel = IsGrounded() ? rabbitAcceleration : rabbitAcceleration * airControl;

        currentMoveVelocity = Vector3.Lerp(currentMoveVelocity, targetMove * rabbitSpeed, accel * Time.deltaTime);
        controller.Move(currentMoveVelocity * Time.deltaTime);

        if (IsGrounded() && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void SwitchState()
    {
        currentState = currentState == MovementState.Human ? MovementState.Rabbit : MovementState.Human;
        Debug.Log("🐇 Movement state switched to: " + currentState);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
