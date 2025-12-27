using UnityEngine;

public class DogBehaviour : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float circleRadius = 2.2f;
    public float circleSpeed = 120f;

    public FirstPersonRabbitController playerController;
    public Animator animator;

    private Transform player;
    private bool approaching;
    private bool circling;
    private float circleAngle;
    private float fixedY;

    public void StartEncounter(Transform playerTransform)
    {
        player = playerTransform;
        approaching = true;
        circling = false;
        circleAngle = 0f;
        fixedY = transform.position.y;

        animator.SetBool("isMoving", true);
        animator.SetBool("isCircling", false);
        animator.SetBool("isTalking", false);
    }

    void Update()
    {
        if (approaching)
            MoveTowardsCircleEntry();

        if (circling)
            CircleAroundPlayer();

        if (circling && Input.GetKeyDown(KeyCode.T))
            StopCircling();
    }

    void MoveTowardsCircleEntry()
    {
        Vector3 toDog = (transform.position - player.position).normalized;
        Vector3 entryPoint = player.position + toDog * circleRadius;
        entryPoint.y = fixedY;

        float dist = Vector3.Distance(transform.position, entryPoint);

        if (dist > 0.05f)
        {
            Vector3 dir = (entryPoint - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            RotateTowards(dir);
        }
        else
        {
            approaching = false;
            circling = true;
            animator.SetBool("isCircling", true);
        }
    }

    void CircleAroundPlayer()
    {
        circleAngle += circleSpeed * Time.deltaTime;
        float rad = circleAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * circleRadius;
        Vector3 targetPos = player.position + offset;
        targetPos.y = fixedY;

        Vector3 moveDir = (targetPos - transform.position).normalized;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        RotateTowards(moveDir);
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
    }

    public void StopCircling()
    {
        circling = false;
        approaching = false;

        animator.SetBool("isCircling", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isTalking", false);

        if (playerController != null)
            playerController.canMove = true;
    }

    public void SetTalking(bool talking)
    {
        animator.SetBool("isTalking", talking);
    }
}
