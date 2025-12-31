using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RabbitRunAwayController : MonoBehaviour
{
    [Header("Movement")]
    public Transform directionHandle;
    public float speed = 5f;
    public float rotationSpeed = 10f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float groundOffset = 0.2f;

    [Header("Identity")]
    public int rabbitIndex;

    [Header("Manager")]
    public RabbitSettlementManager manager;

    [Header("Animation")]
    public Animator animator;
    public string runningBoolName = "isRunning";
    public string diggingBoolName = "isDigging";

    private Vector3 runDirection;
    private bool isSettled = false;
    private bool isCurrentlyRunning = false;

    [HideInInspector]
    public bool isInPlayerTrigger = false;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        if (directionHandle != null)
        {
            Vector3 dir = directionHandle.position - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward;

            runDirection = dir.normalized;
        }

        velocity = Vector3.zero;
    }

    void Update()
    {
        HandleGravity();

        bool shouldRun = !isSettled && isInPlayerTrigger;

        if (animator != null && shouldRun != isCurrentlyRunning)
        {
            animator.SetBool(runningBoolName, shouldRun);
            isCurrentlyRunning = shouldRun;
        }

        if (shouldRun)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        Vector3 move = runDirection * speed * Time.deltaTime;
        controller.Move(move);

        if (runDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(runDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded)
            velocity.y = -groundOffset;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        RabbitSlot slot = other.GetComponent<RabbitSlot>();
        if (slot == null || slot.slotIndex != rabbitIndex)
            return;

        isSettled = true;
        enabled = false;

        if (animator != null && isCurrentlyRunning)
        {
            animator.SetBool(runningBoolName, false);
            isCurrentlyRunning = false;
        }

        if (animator != null)
        {
            animator.SetBool(diggingBoolName, true);
        }

        if (manager != null)
            manager.RabbitSettled(this);
    }
}
