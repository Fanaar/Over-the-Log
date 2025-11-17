using UnityEngine;
using System.Collections;

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
    public float takeOffTime = 2f;
    public float flightGravity = -2f;
    public float initialLiftOffVelocity = 5f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float cameraClampAngle = 85f;
    public Transform cameraPivot;      // Empty object at head/neck
    public Transform playerCamera;     // Main camera under cameraPivot

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("Flight Height Settings")]
    public float maxFlightHeight = 120f;
    public float minFlightHeight = 100f;
    public float ascentSpeed = 5f;
    public float approachSpeed = 2f;

    [Header("Lift-Off Curve")]
    public AnimationCurve liftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float liftDuration = 2f;
    public float maxLiftSpeed = 12f;
    private float liftProgress = 0f;
    private bool isLiftAccelerating = false;

    [Header("Bird Tilt Settings")]
    public float maxTiltAngle = 20f;
    public float tiltSpeed = 5f;
    private float currentTiltZ = 0f;

    [Header("Hunting Settings")]
    public float catchRange = 3f;
    public LayerMask preyLayer;
    public KeyCode diveKey = KeyCode.Mouse0;
    public float diveSpeed = 30f;

    [Header("Bird Look Limits")]
    public float maxYawOffset = 45f; // Max degrees left/right from bird forward

    private CharacterController controller;
    private Vector3 velocity;
    private float birdYaw = 0f; // Fixed flight direction

    private float sprintTimer = 0f;
    public bool canSprint = false;

    [HideInInspector] public bool hasLandedOnPrey = false;
    public PreyDetector preyDetector;

    private bool rotationLocked = false;
    private bool isFlying = false;
    private bool isDiving = false;

    // Camera rotation state
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    void Update()
    {
        HandleMouseLook();
        HandleInteraction();
        HandleInput();
        HandleMovement();
    }

    void HandleInput()
    {
        if (currentState == MovementState.Bird && Input.GetKeyDown(diveKey))
            TryCatchPrey();
    }

    void HandleMovement()
    {
        if (isDiving) return;

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
        if (hasLandedOnPrey) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float speed = (canSprint && Input.GetKey(KeyCode.LeftShift)) ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * speed * Time.deltaTime);

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

        // Use birdYaw as forward
        Quaternion fixedHeading = Quaternion.Euler(0f, birdYaw, 0f);
        Vector3 forwardHeading = fixedHeading * Vector3.forward;
        Vector3 rightHeading = fixedHeading * Vector3.right;

        Vector3 inputDir = rightHeading * moveX + forwardHeading;
        Vector3 horizontalVelocity = inputDir.normalized * flightSpeed;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // Tilt wings visually
        float targetTilt = -moveX * maxTiltAngle;
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSpeed);
        Vector3 localAngles = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(localAngles.x, localAngles.y, currentTiltZ);

        // Maintain flight height
        if (transform.position.y < minFlightHeight)
            velocity.y = ascentSpeed;
        else
            velocity.y = Mathf.Max(velocity.y, 0f);

        if (transform.position.y >= maxFlightHeight && velocity.y > 0f)
            velocity.y *= Mathf.Clamp01(1f - ((transform.position.y - maxFlightHeight) / approachSpeed));

        controller.Move(velocity * Time.deltaTime);
    }

    void TryCatchPrey()
    {
        if (currentState != MovementState.Bird) return;

        if (preyDetector.preyInRange && preyDetector.currentPrey != null)
        {
            PreyMovement prey = preyDetector.currentPrey.GetComponent<PreyMovement>();
            prey.PrepareForStop(0.5f);

            StartCoroutine(DiveToPrey(preyDetector.currentPrey.transform));
            preyDetector.preyInRange = false;
            preyDetector.currentPrey = null;
        }
        else
        {
            Debug.Log("Missed prey!");
        }
    }

    public IEnumerator DiveToPrey(Transform prey)
    {
        isDiving = true;
        rotationLocked = true;
        velocity = Vector3.zero;

        while (prey != null)
        {
            Collider preyCollider = prey.GetComponent<Collider>();
            Vector3 liveTargetPos = preyCollider.bounds.center + Vector3.up * (preyCollider.bounds.extents.y + controller.height * 0.5f);
            transform.position = Vector3.MoveTowards(transform.position, liveTargetPos, diveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, liveTargetPos) < 0.1f)
                break;

            yield return null;
        }

        currentState = MovementState.Human;
        rotationLocked = false;
        hasLandedOnPrey = true;

        Debug.Log("✅ Landed perfectly on prey!");
        isDiving = false;
    }

    void CheckTakeOff()
    {
        if (!canSprint || currentState != MovementState.Human || !controller.isGrounded || !Input.GetKey(KeyCode.LeftShift))
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

        // Initialize bird forward and camera angles
        birdYaw = NormalizeAngle(transform.eulerAngles.y);
        cameraYaw = birdYaw;

        cameraPitch = NormalizeAngle(playerCamera.localEulerAngles.x);
        cameraPivot.localRotation = Quaternion.identity;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Pitch
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -cameraClampAngle, cameraClampAngle);
        playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        if (currentState == MovementState.Human)
        {
            transform.Rotate(Vector3.up * mouseX);
        }
        else if (currentState == MovementState.Bird)
        {
            // Update yaw relative to bird
            cameraYaw += mouseX;
            float relativeYaw = Mathf.DeltaAngle(birdYaw, cameraYaw);
            relativeYaw = Mathf.Clamp(relativeYaw, -maxYawOffset, maxYawOffset);
            cameraYaw = birdYaw + relativeYaw;

            // Only rotate pivot for yaw offset
            cameraPivot.localEulerAngles = new Vector3(0f, relativeYaw, 0f);
        }
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
                hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
        }
    }
}
