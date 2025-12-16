using System;
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

    [Header("Head Bob Settings")]
    public float bobSpeed = 6f;
    public float bobAmount = 0.05f;

    [Header("Rabbit Bob Settings")]
    public float rabbitBobSpeedMultiplier = 1.8f;
    public float rabbitBobAmountMultiplier = 1.4f;

    [Header("Ground Check Settings")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [HideInInspector] public bool canLook = true;
    [HideInInspector] public bool canMove = true;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private float verticalRotation;
    private float defaultCameraY;
    private float bobTimer;

    // --- Player Jump Event ---
    public static event Action OnPlayerJump;

    void Start()
    {
        defaultCameraY = playerCamera.localPosition.y;
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleHeadBob();

        if (canMove)
            HandleMovement();

        if (Input.GetKeyDown(KeyCode.R))
            SwitchState();
    }

    void HandleMouseLook()
    {
        if (!canLook) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -cameraClampAngle, cameraClampAngle);

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.localEulerAngles = Vector3.right * verticalRotation;
    }

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
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // 🔔 Trigger jump event voor konijnen
            OnPlayerJump?.Invoke();
        }

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

    void HandleHeadBob()
    {
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        float speed = bobSpeed;
        float amount = bobAmount;

        if (currentState == MovementState.Rabbit)
        {
            speed *= rabbitBobSpeedMultiplier;
            amount *= rabbitBobAmountMultiplier;
        }

        if (isMoving && IsGrounded())
        {
            bobTimer += Time.deltaTime * speed;

            float bobOffsetY = Mathf.Sin(bobTimer * 1.3f) * amount;
            float bobOffsetX = Mathf.Sin(bobTimer * 0.7f) * amount * 0.4f;

            Vector3 target = new Vector3(0 + bobOffsetX, defaultCameraY + bobOffsetY, 0);

            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, target, Time.deltaTime * 10f);
        }
        else
        {
            bobTimer = 0;
            Vector3 target = new Vector3(0, defaultCameraY, 0);
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, target, Time.deltaTime * 8f);
        }
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
