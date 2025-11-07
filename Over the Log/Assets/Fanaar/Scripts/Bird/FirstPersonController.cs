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
    public float maxFlightHeight = 120f;
    public float minFlightHeight = 100f;
    public float ascentSpeed = 5f;      // how fast you move up/down
    public float approachSpeed = 2f;    // how quickly velocity slows near ceiling/floor

    [Header("Lift-Off Curve (Inspector Controlled)")]
    public AnimationCurve liftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float liftDuration = 2f;     // hoelang de curve duurt
    public float maxLiftSpeed = 12f;    // maximale stijgsnelheid aan het einde
    private float liftProgress = 0f;    // interne timer
    private bool isLiftAccelerating = false;

    [Header("Bird Tilt Settings")]
    public float maxTiltAngle = 20f;    // maximale lean in graden
    public float tiltSpeed = 5f;        // hoe snel de lean volgt
    private float currentTiltZ = 0f; // hou tilt bij

    [HideInInspector] public bool canSprint = false;    // sprint mag pas na trigger
    public bool rotationLocked = false;
    public Quaternion lockedRotation;

    [Header("Hunting Settings")]
    public float catchRange = 3f;          // afstand om prooi te kunnen grijpen
    public LayerMask preyLayer;            // layer waarin de prooi zit
    public KeyCode diveKey = KeyCode.Mouse0;
    public float diveSpeed = 30f; // snelheid van de duik

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;

    private float sprintTimer = 0f;
    private bool justTookOff = false;
    private bool isFlying = false;
    private bool isLanding = false;
    private float birdYaw = 0f;

    public PreyDetector preyDetector; // sleep je trigger hiernaartoe in inspector
    private bool isSprinting => Input.GetKey(KeyCode.LeftShift);
    [HideInInspector] public bool hasLandedOnPrey = false;


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
        if (Input.GetKeyDown(diveKey))
            TryCatchPrey();
    }

    // --------------------------
    // Movement
    // --------------------------

    public void LockRotation(Vector3 euler)
    {
        rotationLocked = true;
        lockedRotation = Quaternion.Euler(euler);
    }

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
        if (hasLandedOnPrey)
            return; // geen beweging meer

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // bepaal snelheid
        float speed = walkSpeed;
        if (canSprint && Input.GetKey(KeyCode.LeftShift))
        {
            speed = sprintSpeed;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
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

        // Auto-forward + strafing
        Vector3 inputDir = transform.right * moveX + transform.forward * 1f;
        Vector3 horizontalVelocity = inputDir.normalized * flightSpeed;
        if (isSprinting)
            horizontalVelocity += transform.forward * flightAcceleration * Time.deltaTime;

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // --- Tilt the bird when strafing (stabilized) ---
        float targetTilt = -moveX * maxTiltAngle;
        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTilt, Time.deltaTime * tiltSpeed);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, currentTiltZ);

        // --- Vertical movement ---
        float targetYVelocity = moveY * ascentSpeed;

        // --- Lift-Off met AnimationCurve ---
        if (isLiftAccelerating)
        {
            liftProgress += Time.deltaTime / liftDuration;
            float t = Mathf.Clamp01(liftProgress);

            float curveValue = liftCurve.Evaluate(t);
            float currentLiftSpeed = Mathf.Lerp(initialLiftOffVelocity, maxLiftSpeed, curveValue);
            velocity.y = currentLiftSpeed;

            if (t >= 1f)
                isLiftAccelerating = false;
        }
        else
        {
            if (transform.position.y < minFlightHeight)
                velocity.y = ascentSpeed;
            else
                velocity.y = targetYVelocity;
        }

        if (transform.position.y >= maxFlightHeight && velocity.y > 0f)
            velocity.y *= Mathf.Clamp01(1f - ((transform.position.y - maxFlightHeight) / approachSpeed));

        controller.Move(velocity * Time.deltaTime);

        if (justTookOff)
            justTookOff = false;
    }
    void TryCatchPrey()
    {
        if (currentState != MovementState.Bird)
            return;

        if (preyDetector.preyInRange && preyDetector.currentPrey != null)
        {
            // Prey stopt 0.5 sec na duik
            PreyMovement prey = preyDetector.currentPrey.GetComponent<PreyMovement>();
            if (prey != null)
                prey.PrepareForStop(0.5f);

            // Start duik
            StartCoroutine(DiveToPrey(preyDetector.currentPrey.transform));

            // reset detector
            preyDetector.preyInRange = false;
            preyDetector.currentPrey = null;

            Debug.Log("Diving to prey!");
        }
        else
        {
            // Gemist → terug omhoog
            velocity = Vector3.up * ascentSpeed;
            Debug.Log("Missed prey!");
        }
    }


    // --------------------------
    // Take-Off & Landing
    // --------------------------
    void CheckTakeOff()
    {
        // Speler mag alleen vliegen als hij ook mag sprinten
        if (!canSprint || currentState != MovementState.Human || !controller.isGrounded || !isSprinting)
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
        justTookOff = true;

        // Start met lage verticale snelheid
        velocity = Vector3.up * initialLiftOffVelocity;

        // Start de lift curve
        liftProgress = 0f;
        isLiftAccelerating = true;
    }

    // --------------------------
    // Mouse Look
    // --------------------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // --- Verticale rotatie ---
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -cameraClampAngle, cameraClampAngle);

        // --- Horizontale rotatie ---
        if (currentState == MovementState.Human)
        {
            if (!rotationLocked)
                transform.Rotate(Vector3.up * mouseX); // normale rotatie
            else
                transform.rotation = lockedRotation;  // gefixeerde rotatie

            playerCamera.localEulerAngles = Vector3.right * verticalRotation;
        }

        else if (currentState == MovementState.Bird)
        {
            // In Bird-modus: camera kan rondkijken, maar beperkt
            birdYaw += mouseX;

            // Limiteer horizontaal zicht, zodat je niet helemaal achterom kunt kijken
            birdYaw = Mathf.Clamp(birdYaw, -75f, 75f); // ← pas deze waarden aan naar wens

            playerCamera.localRotation = Quaternion.Euler(verticalRotation, birdYaw, 0f);
        }

        if (currentState == MovementState.Human)
        {
            if (hasLandedOnPrey)
            {
                // speler staat stil maar kan rondkijken
                transform.Rotate(Vector3.up * mouseX);
            }
            else
            {
                if (!rotationLocked)
                    transform.Rotate(Vector3.up * mouseX);
                else
                    transform.rotation = lockedRotation;
            }

            playerCamera.localEulerAngles = Vector3.right * verticalRotation;
        }

    }
    public IEnumerator DiveToPrey(Transform prey)
    {
        hasLandedOnPrey = false;   // reset landing state
        rotationLocked = true;     // camera locked tijdens duik
        isFlying = true;           // bird mode

        float diveSpeed = 50f;      // snelheid van duik
        float t = 0f;

        // Bereken exact landpunt bovenop de prooi
        Collider preyCollider = prey.GetComponent<Collider>();
        Vector3 targetPos = preyCollider.bounds.center + Vector3.up * (preyCollider.bounds.extents.y + controller.height * 0.5f);

        Vector3 startPos = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f; // kan speed aanpassen
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // Forceer exacte positie, CharacterController tijdelijk uit
        controller.enabled = false;
        transform.position = targetPos;
        controller.enabled = true;

        // ✅ Landing complete
        currentState = MovementState.Human;  // blijf in human mode
        rotationLocked = false;               // camera vrij draaien
        hasLandedOnPrey = true;               // gebruikt door PlayerTriggerLock
        velocity = Vector3.zero;              // stop alle beweging

        Debug.Log("✅ Landed perfectly on prey!");
    }



    public void UnlockRotation()
    {
        rotationLocked = false;
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
}
