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
    public float runSpeed = 5f;

    [Header("Ground Settings")]
    public float gravity = -25f;
    public float groundedSnap = -2f;

    [Header("Jump Reaction")]
    public float jumpDelay = 0.5f;
    public float jumpForce = 6f;

    [Header("Animation")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 targetPosition;
    private Vector3 runTarget;

    private bool isActivated = false;
    private bool isAtDanceSpot = false;
    private int framesAtSpot = 0;

    private float verticalVelocity;
    private bool canJump = false;

    public bool IsAtDanceSpot => isAtDanceSpot;

    // -------------------------
    void OnEnable()
    {
        controller = GetComponent<CharacterController>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        FirstPersonRabbitController.OnPlayerJump += OnPlayerJumped;

        if (gameObject.activeInHierarchy)
            OnActivated();
    }

    void OnDisable()
    {
        FirstPersonRabbitController.OnPlayerJump -= OnPlayerJumped;
    }

    // -------------------------
    void Update()
    {
        ApplyGravity();

        if (isRunningAway)
            HandleRunAway();
        else if (isActivated)
            HandleDanceMovement();

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    // -------------------------
    void HandleDanceMovement()
    {
        if (!isAtDanceSpot)
        {
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;

            Vector3 dir = toTarget.normalized;
            controller.Move(dir * moveSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 5f
                );

            if (toTarget.magnitude < 0.2f)
            {
                framesAtSpot++;
                if (framesAtSpot >= 3)
                {
                    isAtDanceSpot = true;
                    canJump = true;
                }
            }
            else framesAtSpot = 0;
        }
        else
        {
            float angle = 360f / totalRabbits * circleIndex + Time.time * rotationSpeed;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 desiredPos =
                danceSpot.position +
                new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

            Vector3 move = desiredPos - transform.position;
            move.y = 0f;

            controller.Move(move * orbitSmoothSpeed * Time.deltaTime);

            Vector3 tangent = -Vector3.Cross(Vector3.up, (transform.position - danceSpot.position).normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(tangent),
                Time.deltaTime * 5f
            );
        }
    }

    // -------------------------
    void HandleRunAway()
    {
        Vector3 toTarget = runTarget - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.1f)
            return;

        Vector3 dir = toTarget.normalized;

        controller.Move(dir * runSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 8f
        );
    }

    // -------------------------
    void ApplyGravity()
    {
        if (controller.isGrounded)
            verticalVelocity = groundedSnap;
        else
            verticalVelocity += gravity * Time.deltaTime;
    }

    void OnPlayerJumped()
    {
        if (!canJump || !controller.isGrounded) return;
        StartCoroutine(JumpAfterDelay());
    }

    IEnumerator JumpAfterDelay()
    {
        yield return new WaitForSeconds(jumpDelay);
        verticalVelocity = jumpForce;
        animator?.SetTrigger("JumpTrigger");
    }

    // -------------------------
    public void OnActivated()
    {
        if (isActivated) return;
        isActivated = true;

        float angle = 360f / totalRabbits * circleIndex;
        float rad = angle * Mathf.Deg2Rad;

        targetPosition =
            danceSpot.position +
            new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
    }

    public void RunAwayTo(Vector3 worldTarget)
    {
        isRunningAway = true;
        runTarget = worldTarget;

        canJump = false;
        isAtDanceSpot = false;
        framesAtSpot = 0;
    }
}
