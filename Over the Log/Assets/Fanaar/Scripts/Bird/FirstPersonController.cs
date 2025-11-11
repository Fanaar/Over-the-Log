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
    public Transform playerCamera;
    public float cameraClampAngle = 85f;

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

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private float birdYaw = 0f;

    private float sprintTimer = 0f;
    public bool canSprint = false;

    [HideInInspector] public bool hasLandedOnPrey = false;
    public PreyDetector preyDetector;

    private bool rotationLocked = false;
    private Quaternion lockedRotation;

    private bool isFlying = false;

    private bool isDiving = false;


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
    // Input / Movement Flow
    // --------------------------
    void HandleInput()
    {
        if (currentState == MovementState.Bird && Input.GetKeyDown(diveKey))
            TryCatchPrey();
    }

    void HandleMovement()
    {
        if (isDiving) return; // ✅ stop moving while diving

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

        Vector3 inputDir = transform.right * moveX + transform.forward * 1f;
        Vector3 horizontalVelocity = inputDir.normalized * flightSpeed;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        float targetTilt = -moveX * maxTiltAngle;
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, currentTiltZ);

        if (transform.position.y < minFlightHeight)
            velocity.y = ascentSpeed;
        else
            velocity.y = (velocity.y > 0) ? velocity.y : 0;

        if (transform.position.y >= maxFlightHeight && velocity.y > 0f)
            velocity.y *= Mathf.Clamp01(1f - ((transform.position.y - maxFlightHeight) / approachSpeed));

        controller.Move(velocity * Time.deltaTime);
    }

    // --------------------------
    // Flying / Dive to prey
    // --------------------------
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

    // ✅ Updated dive with homing + no disabling CharacterController
    public IEnumerator DiveToPrey(Transform prey)
    {
        isDiving = true;
        rotationLocked = true;
        velocity = Vector3.zero;

        while (prey != null)
        {
            Collider preyCollider = prey.GetComponent<Collider>();

            Vector3 liveTargetPos = preyCollider.bounds.center +
                Vector3.up * (preyCollider.bounds.extents.y + controller.height * 0.5f);

            // ✅ Uses the inspector value:
            transform.position = Vector3.MoveTowards(transform.position, liveTargetPos, diveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, liveTargetPos) < 0.1f)
                break;

            yield return null;
        }

        currentState = MovementState.Human;
        rotationLocked = false;
        hasLandedOnPrey = true;

        Debug.Log("✅ Landed perfectly on prey!");

        isDiving = false; // ✅ movement re-enabled
    }


    // --------------------------
    // Take-Off / Mouse Look / Interaction
    // --------------------------

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
        liftProgress = 0f;
        isLiftAccelerating = true;
    }

    void HandleMouseLook()
    {
        if (rotationLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -cameraClampAngle, cameraClampAngle);

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.localEulerAngles = Vector3.right * verticalRotation;
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
