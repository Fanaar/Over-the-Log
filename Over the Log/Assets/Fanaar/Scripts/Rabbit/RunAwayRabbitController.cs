using UnityEngine;

public class RabbitRunAwayController : MonoBehaviour
{
    [Header("Movement")]
    public Transform directionHandle;
    public float speed = 5f;

    [Header("Identity")]
    public int rabbitIndex;

    [Header("Manager")]
    public RabbitSettlementManager manager;

    [Header("Animation")]
    public Animator animator;
    public string runningBoolName = "isRunning"; // gewoon de naam van je bool in de animator

    private Vector3 runDirection;
    private bool isSettled = false;

    [HideInInspector]
    public bool isInPlayerTrigger = false;

    private bool isCurrentlyRunning = false; // interne flag om te checken of animator al loopt

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
    }

    void Update()
    {
        bool shouldRun = !isSettled && isInPlayerTrigger;

        // Update animator alleen als toestand verandert
        if (animator != null && shouldRun != isCurrentlyRunning)
        {
            animator.SetBool(runningBoolName, shouldRun);
            isCurrentlyRunning = shouldRun;
        }

        if (!shouldRun)
            return;

        // Beweging
        transform.position += runDirection * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(runDirection);
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

        if (manager != null)
            manager.RabbitSettled(this);
    }
}