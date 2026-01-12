using UnityEngine;

public class DogController2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Animation")]
    public Animator animator;          // reference to the dog's Animator

    [HideInInspector]
    public bool isInPlayerTrigger = false;

    [Header("Stress Settings")]
    public float maxStressSpeed = 9f; // maximale hondensnelheid

    private float debugTimer = 0f;
    public float debugInterval = 0.1f; // print 10 keer per seconde


    [Header("Grounding")]
    public LayerMask groundLayer;
    public float groundCheckHeight = 2f;
    public float groundSnapSpeed = 10f;
    public float groundOffset = 0.5f; // height from pivot to feet


    private void Update()
    {
        if (!gameObject.activeInHierarchy || player == null)
            return;

        // Stress-factor en dynamicSpeed overal beschikbaar
        float stressFactor = StressManager.Instance != null ? StressManager.Instance.stress / 100f : 0f;
        float dynamicSpeed = Mathf.Lerp(speed * 0.6f, maxStressSpeed, stressFactor);
        bool playerIsMoving = StressManager.Instance != null && StressManager.Instance.playerIsMoving;

        if (isInPlayerTrigger && !playerIsMoving)
        {
            if (StressManager.Instance != null)
                StressManager.Instance.playerIsLookingAtDog = true;

            animator.SetBool("isWalking", false);
            return;
        }

        else
        {
            if (StressManager.Instance != null)
                StressManager.Instance.playerIsLookingAtDog = false;
        }


        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            // Rotate towards player
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Move
            transform.position += transform.forward * dynamicSpeed * Time.deltaTime;

            // Walking animation
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        // ----- DEBUG -----
        debugTimer += Time.deltaTime;
        if (debugTimer >= debugInterval)
        {
            float stressLevel = StressManager.Instance != null ? StressManager.Instance.stress : 0f;
            Debug.Log($"[DEBUG] Stress: {stressLevel:F1} | DogSpeed: {dynamicSpeed:F2}");
            debugTimer = 0f;
        }
        // ----- EIND DEBUG -----
        StickToGround();
    }
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
