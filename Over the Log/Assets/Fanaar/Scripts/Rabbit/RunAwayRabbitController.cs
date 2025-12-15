using UnityEngine;

public class RabbitRunAwayController : MonoBehaviour
{
    [Header("Movement")]
    public Transform directionHandle;
    public float speed = 5f;

    [Header("Identity")]
    public int rabbitIndex;

    private Vector3 runDirection;
    private bool isSettled = false;

    [HideInInspector]
    public bool isInPlayerTrigger = false; // Nieuw: in trigger bij speler

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
        if (slot == null) return;
        if (slot.slotIndex != rabbitIndex) return;

        // Konijn is aangekomen
        isSettled = true;
        enabled = false; // HARD STOP
    }
}
