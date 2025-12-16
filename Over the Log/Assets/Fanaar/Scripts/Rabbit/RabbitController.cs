using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RabbitController : MonoBehaviour
{
    [Header("Circle Settings")]
    public Transform danceSpot;
    public float radius = 1.5f;
    public float rotationSpeed = 30f;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float orbitSmoothSpeed = 2f;

    [Header("Circle Index")]
    public int circleIndex = 0;
    public int totalRabbits = 1;

    [Header("Run Away Settings")]
    public bool isRunningAway = false;
    public Vector3 runDirection = Vector3.forward;
    public float runSpeed = 5f;
    public float spreadAmount = 1f;

    [Header("Ground Settings")]
    public LayerMask groundLayer;
    public float gravity = -25f;
    public float groundedSnap = -2f;

    [Header("Jump Reaction")]
    public float jumpDelay = 0.5f;
    public float jumpForce = 6f;

    private CharacterController controller;
    private Vector3 targetPosition;

    private bool isActivated = false;
    private bool isAtDanceSpot = false;

    // vertical motion
    private float verticalVelocity;
    private bool isJumping;

    public bool IsAtDanceSpot => isAtDanceSpot;

    // -------------------------
    // LIFECYCLE
    // -------------------------
    void OnEnable()
    {
        controller = GetComponent<CharacterController>();
        FirstPersonRabbitController.OnPlayerJump += OnPlayerJumped;

        if (gameObject.activeInHierarchy)
            OnActivated();
    }

    void OnDisable()
    {
        FirstPersonRabbitController.OnPlayerJump -= OnPlayerJumped;
    }

    // -------------------------
    // UPDATE
    // -------------------------
    void Update()
    {
        ApplyGravity();

        if (isRunningAway)
        {
            HandleRunAway();
        }
        else if (isActivated)
        {
            HandleDanceMovement();
        }

        // Apply vertical + horizontal movement together
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // -------------------------
    // MOVEMENT
    // -------------------------
    void HandleDanceMovement()
    {
        if (!isAtDanceSpot)
        {
            Vector3 dir = (targetPosition - transform.position).normalized;
            controller.Move(dir * moveSpeed * Time.deltaTime);
            transform.LookAt(danceSpot);

            if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
                isAtDanceSpot = true;
        }
        else
        {
            float angle = 360f / totalRabbits * circleIndex + Time.time * rotationSpeed;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 desiredPos = danceSpot.position +
                                 new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

            Vector3 move = (desiredPos - transform.position);
            move.y = 0f;

            controller.Move(move * orbitSmoothSpeed * Time.deltaTime);

            Vector3 radiusDir = (transform.position - danceSpot.position).normalized;
            Vector3 tangentDir = -Vector3.Cross(Vector3.up, radiusDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(tangentDir),
                Time.deltaTime * 5f
            );
        }
    }

    void HandleRunAway()
    {
        Vector3 offset = Vector3.right * ((circleIndex - totalRabbits / 2f) * spreadAmount);
        Vector3 dir = (runDirection.normalized + offset).normalized;

        controller.Move(dir * runSpeed * Time.deltaTime);

        if (dir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 5f
            );
        }
    }

    // -------------------------
    // GRAVITY + JUMP
    // -------------------------
    void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = groundedSnap;

            isJumping = false;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    void OnPlayerJumped()
    {
        if (!gameObject.activeInHierarchy || isJumping)
            return;

        StartCoroutine(JumpAfterDelay());
    }

    IEnumerator JumpAfterDelay()
    {
        yield return new WaitForSeconds(jumpDelay);

        if (!controller.isGrounded)
            yield break;

        verticalVelocity = jumpForce;
        isJumping = true;
    }

    // -------------------------
    // PUBLIC API
    // -------------------------
    public void OnActivated()
    {
        if (isActivated) return;
        isActivated = true;

        float angle = 360f / totalRabbits * circleIndex;
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        targetPosition = danceSpot.position + offset;
    }

    public void RunAway(Vector3 direction)
    {
        isRunningAway = true;
        runDirection = direction.normalized;
    }
}
