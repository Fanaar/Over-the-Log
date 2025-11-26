using UnityEngine;

public class DogController : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1.5f;

    [Header("Grounding")]
    public float groundOffset = 0.2f;      // keeps dog slightly above terrain
    public float groundCheckDistance = 5f; // how far the raycast checks downward

    private Transform target;
    private bool isChasing = false;

    void Update()
    {
        if (!isChasing || target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0; // ignore vertical difference for movement
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            Debug.Log("🐶 Dog reached player!");
            // trigger caught logic here
        }

        StickToGround();
    }

    private void StickToGround()
    {
        // raycast down to find terrain height
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + groundOffset;
            transform.position = pos;
        }
    }

    public void StartChase(Transform chaseTarget)
    {
        target = chaseTarget;
        isChasing = true;
        Debug.Log("🐶 Dog starts chasing " + target.name);
    }

    public void StopChase()
    {
        isChasing = false;
    }
}
