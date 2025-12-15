using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DogController : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1.5f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundOffset = 0.2f;

    private Transform target;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake() => controller = GetComponent<CharacterController>();

    void Start()
    {
        StartCoroutine(DelayedSnap());
    }

    void Update()
    {
        HandleGravity();

        if (isChasing && target != null)
            HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            controller.Move(dir.normalized * moveSpeed * Time.deltaTime);

            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (!hasCaughtPlayer)
        {
            hasCaughtPlayer = true;
            Debug.Log("🐶 Dog reached player!");
            // Hier kan eventueel een event of callback komen voor wat er moet gebeuren
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

    public void StartChase(Transform chaseTarget)
    {
        target = chaseTarget;
        isChasing = true;
        hasCaughtPlayer = false;
        Debug.Log("🐶 Dog starts chasing " + chaseTarget.name);
    }

    public void StopChase() => isChasing = false;

    public void ForceGroundSnap()
    {
        if (controller == null) return;

        controller.enabled = false;

        Vector3 rayStart = transform.position + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 50f))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + groundOffset + controller.center.y;
            transform.position = pos;

            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }
        else
        {
            Debug.LogWarning("DogController: No ground detected under spawn point!");
        }

        controller.enabled = true;
    }

    private IEnumerator DelayedSnap()
    {
        yield return null;
        yield return null;
        ForceGroundSnap();
    }
}
