using UnityEngine;

public class DogController2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Animation")]
    public Animator animator;

    [HideInInspector]
    public bool isInPlayerTrigger = false;

    [Header("Stress Settings")]
    public float maxStressSpeed = 9f;

    [Header("Grounding")]
    public LayerMask groundLayer;
    public float groundCheckHeight = 2f;
    public float groundSnapSpeed = 10f;
    public float groundOffset = 0.5f;

    // =========================
    // Internal state
    // =========================
    private bool isInAcceptanceSit = false;

    void Update()
    {
        if (!gameObject.activeInHierarchy || player == null)
            return;

        StressManager stressManager = StressManager.Instance;
        bool playerIsMoving = stressManager != null && stressManager.playerIsMoving;

        // =====================================================
        // 1️⃣ ACCEPTANCE SIT (HARD LOCK)
        // =====================================================
        if (isInAcceptanceSit)
        {
            animator.SetBool("isWalking", false);

            if (playerIsMoving)
            {
                isInAcceptanceSit = false;
                animator.SetBool("isSitting", false);
            }

            StickToGround();
            return;
        }

        // =====================================================
        // 2️⃣ PLAYER STIL + KIJKEN = STOPPEN (IDLE)
        // =====================================================
        if (isInPlayerTrigger && !playerIsMoving)
        {
            if (stressManager != null)
                stressManager.playerIsLookingAtDog = true;

            animator.SetBool("isWalking", false);

            // ---- ENTER acceptance sit
            if (stressManager != null && stressManager.AcceptanceSitReached)
            {
                isInAcceptanceSit = true;
                animator.SetBool("isSitting", true);
            }

            StickToGround();
            return;
        }

        // =====================================================
        // 3️⃣ NORMAAL VOLGEN / LOPEN
        // =====================================================
        if (stressManager != null)
            stressManager.playerIsLookingAtDog = false;

        HandleMovement(stressManager);
    }

    // =========================
    // Movement logic
    // =========================
    void HandleMovement(StressManager stressManager)
    {
        float stressFactor = stressManager != null ? stressManager.stress / 100f : 0f;
        float dynamicSpeed = Mathf.Lerp(speed * 0.6f, maxStressSpeed, stressFactor);

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );

            transform.position += transform.forward * dynamicSpeed * Time.deltaTime;
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        StickToGround();
    }

    // =========================
    // Ground snapping
    // =========================
    void StickToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckHeight * 2f, groundLayer))
        {
            Vector3 targetPos = transform.position;
            targetPos.y = hit.point.y + groundOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPos,
                groundSnapSpeed * Time.deltaTime
            );
        }
    }
}
