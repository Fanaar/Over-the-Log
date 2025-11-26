using UnityEngine;

public class DogController : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1.5f;

    private Transform target;
    private bool isChasing = false;

    void Update()
    {
        if (!isChasing || target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;
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
