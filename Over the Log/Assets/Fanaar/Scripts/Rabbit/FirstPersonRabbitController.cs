using System;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonRabbitController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintMultiplier = 1.6f;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float cameraClampAngle = 85f;

    [Header("Head Bob")]
    public float bobSpeed = 6f;
    public float bobAmount = 0.05f;

    [Header("Footstep Audio")]
    public EventReference footstepEvent;
    public float footstepThreshold = -0.01f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [HideInInspector] public bool canLook = true;
    [HideInInspector] public bool canMove = true;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation;
    private float defaultCameraY;
    private float bobTimer;
    private float lastBobY;

    public static event Action OnPlayerJump;

    public bool isMoving;
    private bool isSprinting;

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

        ReportStressInputs();
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
        bool grounded = IsGrounded();

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * inputX + transform.forward * inputZ).normalized;

        bool isSprinting = Input.GetKey(sprintKey) && inputZ > 0 && grounded;
        this.isSprinting = isSprinting;
        this.isMoving = move.magnitude > 0.1f;

        float speed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            OnPlayerJump?.Invoke();
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    void HandleHeadBob()
    {
        if (!canMove)
        {
            ResetHeadBob();
            return;
        }

        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        float speed = bobSpeed;
        float amount = bobAmount;

        if (Input.GetKey(sprintKey))
        {
            speed *= 1.5f;
            amount *= 1.2f;
        }

        if (isMoving && IsGrounded())
        {
            bobTimer += Time.deltaTime * speed;

            float bobOffsetY = Mathf.Sin(bobTimer * 1.3f) * amount;
            float bobOffsetX = Mathf.Sin(bobTimer * 0.7f) * amount * 0.4f;

            if (lastBobY > footstepThreshold && bobOffsetY <= footstepThreshold)
                RuntimeManager.PlayOneShot(footstepEvent, transform.position);

            lastBobY = bobOffsetY;

            Vector3 target = new Vector3(bobOffsetX, defaultCameraY + bobOffsetY, 0);
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, target, Time.deltaTime * 10f);
        }
        else
        {
            ResetHeadBob();
        }
    }

    void ResetHeadBob()
    {
        bobTimer = 0;
        lastBobY = 0;

        Vector3 target = new Vector3(0, defaultCameraY, 0);
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, target, Time.deltaTime * 8f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    void ReportStressInputs()
    {
        if (StressManager.Instance == null) return;

        // Check of speler beweegt / sprint
        StressManager.Instance.playerIsMoving = isMoving;
        StressManager.Instance.playerIsSprinting = isSprinting;
    }

}
