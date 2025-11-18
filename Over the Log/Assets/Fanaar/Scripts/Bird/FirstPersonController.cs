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
    public Transform cameraPivot;
    public Transform playerCamera;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("Flight Height Settings")]
    public float maxFlightHeight = 120f;
    public float minFlightHeight = 100f;
    public float ascentSpeed = 5f;

    [Header("Bird Tilt Settings")]
    public float maxTiltAngle = 20f;
    public float tiltSpeed = 5f;
    private float currentTiltZ = 0f;

    [Header("Hunting Settings")]
    public KeyCode diveKey = KeyCode.Mouse0;
    public float diveSpeed = 30f;
    public float diveRotationSpeed = 5f;
    public float diveStopDistance = 0.5f;
    public AudioClip diveSound;

    [Header("Bird Look Limits")]
    public float maxYawOffset = 45f;

    [Header("Prey Detection Triggers")]
    public PreyDetector catchTrigger; // inner trigger
    public PreyDetector missTrigger;  // outer trigger

    [Header("Guided Camera Pan")]
    public float guidedPanDuration = 1f;        // time to pan toward prey
    public float guidedPanReturnDuration = 0.5f; // time to return

    private CharacterController controller;
    private Vector3 velocity;
    private float birdYaw = 0f;

    private float sprintTimer = 0f;
    public bool canSprint = false;

    [HideInInspector] public bool hasLandedOnPrey = false;

    private bool rotationLocked = false;
    private bool isFlying = false;
    private bool isDiving = false;

    private float cameraPitch = 0f;
    private float cameraYaw = 0f;

    // Guided pan variables
    private bool isGuidedPanning = false;
    private bool isReturningFromPan = false;
    private float panTimer = 0f;
    private Quaternion panStartRot;
    private Quaternion panTargetRot;
    private Transform lastGuidedPrey = null;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Update guided pan first
        if (isGuidedPanning)
            UpdateGuidedPan();
        else
            HandleMouseLook();

        HandleInteraction();
        HandleInput();
        HandleMovement();

        // Lock X and Z rotation when human
        if (currentState == MovementState.Human)
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        // Check outer trigger for guided pan
        if (currentState == MovementState.Bird && !isDiving && !isGuidedPanning)
            CheckForPreyGuidedPan();
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
        float verticalInput = Input.GetAxis("Vertical");

        Quaternion fixedHeading = Quaternion.Euler(0f, birdYaw, 0f);
        Vector3 forwardHeading = fixedHeading * Vector3.forward;
        Vector3 rightHeading = fixedHeading * Vector3.right;

        Vector3 forwardVelocity = forwardHeading * flightSpeed;
        Vector3 strafeVelocity = rightHeading * moveX * flightSpeed;

        velocity.x = forwardVelocity.x + strafeVelocity.x;
        velocity.z = forwardVelocity.z + strafeVelocity.z;

        if (verticalInput > 0f && transform.position.y >= maxFlightHeight)
            velocity.y = 0f;
        else if (verticalInput < 0f && transform.position.y <= minFlightHeight)
            velocity.y = 0f;
        else
            velocity.y = verticalInput * ascentSpeed;

        float targetTilt = -moveX * maxTiltAngle;
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSpeed);
        Vector3 localAngles = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(localAngles.x, localAngles.y, currentTiltZ);

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -cameraClampAngle, cameraClampAngle);
        playerCamera.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

        if (currentState == MovementState.Human)
            transform.Rotate(Vector3.up * mouseX);
        else if (currentState == MovementState.Bird)
        {
            cameraYaw += mouseX;
            float relativeYaw = Mathf.DeltaAngle(birdYaw, cameraYaw);
            relativeYaw = Mathf.Clamp(relativeYaw, -maxYawOffset, maxYawOffset);
            cameraYaw = birdYaw + relativeYaw;
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
        birdYaw = NormalizeAngle(transform.eulerAngles.y);
        cameraYaw = birdYaw;
        cameraPitch = NormalizeAngle(playerCamera.localEulerAngles.x);
        cameraPivot.localRotation = Quaternion.identity;
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    void TryCatchPrey()
    {
        if (currentState != MovementState.Bird) return;

        if (catchTrigger.preyInRange && catchTrigger.currentPrey != null)
        {
            PreyMovement prey = catchTrigger.currentPrey.GetComponent<PreyMovement>();
            if (prey != null) prey.OnCaught();
            StartCoroutine(DiveToPrey(catchTrigger.currentPrey.transform));
        }
        else if (missTrigger.preyInRange && missTrigger.currentPrey != null)
        {
            Debug.Log("Near miss!");
        }
        else
        {
            Debug.Log("Too far to dive!");
        }
    }

    public IEnumerator DiveToPrey(Transform prey)
    {
        isDiving = true;
        rotationLocked = true;
        velocity = Vector3.zero;

        if (diveSound != null)
            AudioSource.PlayClipAtPoint(diveSound, transform.position);

        PreyMovement preyScript = prey.GetComponent<PreyMovement>();
        if (preyScript != null)
            preyScript.OnCaught();

        while (prey != null)
        {
            Collider preyCollider = prey.GetComponent<Collider>();
            Vector3 liveTargetPos = preyCollider != null
                ? preyCollider.bounds.center + Vector3.up * (preyCollider.bounds.extents.y + controller.height * 0.5f)
                : prey.position;

            transform.position = Vector3.MoveTowards(transform.position, liveTargetPos, diveSpeed * Time.deltaTime);

            Vector3 dir = (liveTargetPos - transform.position);
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * diveRotationSpeed);
            }

            if (Vector3.Distance(transform.position, liveTargetPos) < diveStopDistance)
                break;

            yield return null;
        }

        currentState = MovementState.Human;
        rotationLocked = false;
        hasLandedOnPrey = true;

        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        cameraPivot.localRotation = Quaternion.identity;
        cameraPitch = 0f;
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        isDiving = false;
    }

    // ----------------------
    // Guided Camera Pan (3D toward prey, fixed)
    // ----------------------
    void CheckForPreyGuidedPan()
    {
        Transform targetPrey = null;

        if (missTrigger.preyInRange && missTrigger.currentPrey != null)
            targetPrey = missTrigger.currentPrey.transform;

        if (targetPrey != null && targetPrey != lastGuidedPrey)
        {
            StartGuidedPan(targetPrey);
            lastGuidedPrey = targetPrey;
        }

        if (targetPrey == null)
            lastGuidedPrey = null;
    }

    void StartGuidedPan(Transform prey)
    {
        isGuidedPanning = true;
        isReturningFromPan = false;
        panTimer = 0f;

        panStartRot = cameraPivot.localRotation;

        // Correct: compute local rotation relative to player
        Vector3 dirToPrey = prey.position - cameraPivot.position;
        Vector3 localDir = transform.InverseTransformDirection(dirToPrey);
        panTargetRot = Quaternion.LookRotation(localDir, Vector3.up);
    }

    void UpdateGuidedPan()
    {
        panTimer += Time.deltaTime;

        if (!isReturningFromPan)
        {
            float t = Mathf.Clamp01(panTimer / guidedPanDuration);
            cameraPivot.localRotation = Quaternion.Slerp(panStartRot, panTargetRot, t);

            if (t >= 1f)
            {
                isReturningFromPan = true;
                panTimer = 0f;
            }
        }
        else
        {
            float t = Mathf.Clamp01(panTimer / guidedPanReturnDuration);
            cameraPivot.localRotation = Quaternion.Slerp(panTargetRot, panStartRot, t);

            if (t >= 1f)
                isGuidedPanning = false;
        }
    }
}
