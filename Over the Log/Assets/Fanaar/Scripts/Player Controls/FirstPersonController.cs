using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public enum MovementState { Human, Bird }
    public MovementState currentState = MovementState.Human;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Bird Settings")]
    public float flightSpeed = 10f;
    public float flightAcceleration = 20f;
    public float takeOffTime = 2f; // seconds sprinting before lift-off
    public float flightGravity = -2f;
    public float initialLiftOffVelocity = 5f;
    //public float maxFlightHeight = 20f; // default maximum flight height

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public float cameraClampAngle = 85f;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("Flight Height Settings")]
    public float maxFlightHeight = 50f;
    public float minFlightHeight = 5f;
    public float ascentSpeed = 5f;      // how fast you move up/down
    public float approachSpeed = 2f;    // how quickly velocity slows near ceiling/floor


    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    private float sprintTimer = 0f;
    private bool justTookOff = false;
    private bool isFlying = false;
    private bool isLanding = false;

    private bool isSprinting => Input.GetKey(KeyCode.LeftShift);

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleInteraction();
        HandleInput();
        HandleMovement();
    }

    // --------------------------
    // Input Handling
    // --------------------------
    void HandleInput()
    {
        if (currentState == MovementState.Bird)
            HandleBirdInput();
    }

    void HandleBirdInput()
    {
        // Manual landing
        if (Input.GetKeyDown(KeyCode.L))
            Land();
    }

    // --------------------------
    // Movement
    // --------------------------
    void HandleMovement()
    {
        switch (currentState)
        {
            case MovementState.Human:
                HandleHumanMovement();
                CheckTakeOff();
                break;
            case MovementState.Bird:
                HandleBirdMovement();
                break;
        }
    }

    void HandleHumanMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        // Gravity & jump
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleBirdMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = 0f;

        // W = up, S = down (disabled during landing)
        if (!isLanding)
        {
            if (Input.GetKey(KeyCode.W)) moveY = 1f;
            if (Input.GetKey(KeyCode.S)) moveY = -1f;
        }

        // Auto-forward + strafing
        Vector3 inputDir = transform.right * moveX + transform.forward * 1f;
        Vector3 horizontalVelocity = inputDir.normalized * flightSpeed;
        if (isSprinting)
            horizontalVelocity += transform.forward * flightAcceleration * Time.deltaTime;

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // Vertical movement
        float targetYVelocity = moveY * ascentSpeed;

        // Smoothly reduce vertical speed near ceiling/floor
        if (transform.position.y >= maxFlightHeight && targetYVelocity > 0f)
            targetYVelocity *= Mathf.Clamp01(1f - ((transform.position.y - maxFlightHeight) / approachSpeed));
        if (transform.position.y <= minFlightHeight && targetYVelocity < 0f)
            targetYVelocity *= Mathf.Clamp01(1f - ((minFlightHeight - transform.position.y) / approachSpeed));

        // Gradual landing
        if (isLanding)
        {
            // Slowly reduce upward velocity
            targetYVelocity = Mathf.Max(-ascentSpeed, velocity.y - 9.81f * Time.deltaTime); // simulate gravity
            if (controller.isGrounded)
            {
                // Finished landing
                isLanding = false;
                currentState = MovementState.Human;
                velocity = Vector3.zero;
                sprintTimer = 0f;
            }
        }

        velocity.y = targetYVelocity;

        // Move player
        controller.Move(velocity * Time.deltaTime);

        if (justTookOff)
            justTookOff = false;
    }


    // --------------------------
    // Take-Off & Landing
    // --------------------------
    void CheckTakeOff()
    {
        if (currentState != MovementState.Human || !controller.isGrounded || !isSprinting)
        {
            sprintTimer = 0f;
            return;
        }

        sprintTimer += Time.deltaTime;
        if (sprintTimer >= takeOffTime)
            StartFlying();
    }

    void StartFlying()
    {
        currentState = MovementState.Bird;
        isFlying = true;
        velocity = Vector3.up * initialLiftOffVelocity;
        justTookOff = true;
    }

    void Land()
    {
        isLanding = true; // start gradual descent
        currentState = MovementState.Bird; // stay in bird state during landing
    }

    // --------------------------
    // Mouse Look
    // --------------------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -cameraClampAngle, cameraClampAngle);

        playerCamera.localEulerAngles = Vector3.right * verticalRotation;
        transform.Rotate(Vector3.up * mouseX);
    }

    // --------------------------
    // Interaction
    // --------------------------
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // --------------------------
    // Flight Zone Trigger Example
    // --------------------------
    private void OnTriggerEnter(Collider other)
    {
        // Example: adjust max flight height dynamically
        if (other.CompareTag("FlightZone1"))
            maxFlightHeight = 25f;
        else if (other.CompareTag("FlightZone2"))
            maxFlightHeight = 40f;
        else if (other.CompareTag("FlightZone3"))
            maxFlightHeight = 60f;
    }
}
