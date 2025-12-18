using UnityEngine;

public class RabbitRunAwayController : MonoBehaviour
{
    [Header("Movement")]
    public Transform directionHandle;
    public float speed = 5f;

    [Header("Identity")]
    public int rabbitIndex;

    [Header("Manager")]
    public RabbitSettlementManager manager; // sleep hier de manager in inspector

    private Vector3 runDirection;
    private bool isSettled = false;

    [HideInInspector]
    public bool isInPlayerTrigger = false; // of hij binnen de spelertrigger is

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
        if (isSettled || !isInPlayerTrigger)
            return; // Stoppen als gesettled of niet in trigger

        transform.position += runDirection * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(runDirection);
    }

    void OnTriggerEnter(Collider other)
    {
        RabbitSlot slot = other.GetComponent<RabbitSlot>();
        if (slot == null || slot.slotIndex != rabbitIndex)
            return;

        // Konijn bereikt zijn slot
        isSettled = true;
        enabled = false;

        // Notify manager (veilig tegen dubbel tellen)
        if (manager != null)
            manager.RabbitSettled(this);
    }
}
