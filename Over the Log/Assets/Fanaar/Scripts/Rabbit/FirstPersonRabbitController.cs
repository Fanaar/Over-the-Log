using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonRabbitController : MonoBehaviour
{
    public enum MovementState { Human, Rabbit }
    public MovementState currentState = MovementState.Human;
    public GroundCheck groundCheck; // assign in Inspector


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

    [Header("Beat Settings")]
    public float beatForgiveness = 0.15f; // how early/late jump still counts as "on beat"

    [Header("On-Beat Visual Effect")]
    public ParticleSystem onBeatEffect;  // Drag your existing effect here in the Inspector

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMoveVelocity;
    private float verticalRotation;
    private float lastBeatTime;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();

        if (Input.GetKeyDown(KeyCode.R))
            SwitchState();
    }

    void OnEnable() => RabbitCircleBeatRotator.OnBeat += RegisterBeat;
    void OnDisable() => RabbitCircleBeatRotator.OnBeat -= RegisterBeat;

    void RegisterBeat()
    {
        lastBeatTime = Time.time;
    }

    // --------------------------
    // MOUSE LOOK
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

    void HumanMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;
        controller.Move(move * walkSpeed * Time.deltaTime);

        bool grounded = groundCheck.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && grounded)
        {
            bool isOnBeat = Mathf.Abs(Time.time - lastBeatTime) <= beatForgiveness;

            if (isOnBeat)
            {
                Debug.Log("✅ Jump on beat!");
                if (onBeatEffect) onBeatEffect.Play();
            }
            else
            {
                Debug.Log("❌ Jump off beat");
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

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

        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void SwitchState()
    {
        currentState = currentState == MovementState.Human ? MovementState.Rabbit : MovementState.Human;
        Debug.Log("🐇 Movement state switched to: " + currentState);
    }
}
