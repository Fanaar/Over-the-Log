using UnityEngine;

public class RabbitIntroController : MonoBehaviour
{
    [Header("Movement")]
    public Transform targetPosition;
    public float moveSpeed = 2f;
    public float stopDistance = 0.05f;

    [Header("Rotation")]
    public float rotationSpeed = 8f;

    [Header("Animation")]
    public Animator animator;

    private bool isMoving = false;

    public void StartIntro()
    {
        isMoving = true;

        if (animator != null)
            animator.SetBool("isHopping", true);
    }

    private void Update()
    {
        if (!isMoving || targetPosition == null) return;

        RotateTowardsTarget();
        MoveTowardsTarget();

        if (Vector3.Distance(transform.position, targetPosition.position) <= stopDistance)
        {
            Arrived();
        }
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition.position,
            moveSpeed * Time.deltaTime
        );
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = targetPosition.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void Arrived()
    {
        isMoving = false;

        if (animator != null)
            animator.SetBool("isHopping", false);

        gameObject.SetActive(false);
    }
}
