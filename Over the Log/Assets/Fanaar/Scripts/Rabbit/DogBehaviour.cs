using UnityEngine;

public class DogBehaviour : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float circleRadius = 2.2f;
    public float circleSpeed = 120f;
    public float finalApproachDuration = 1f;
    public float stopDistance = 0.6f;

    [Header("Circle Direction")]
    public bool clockwise = true;

    [Header("References")]
    public FirstPersonRabbitController playerController;
    public Animator animator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] circlingClips;
    public float audioDelay = 2f;
    public float talkingDelay = 2f;

    private float circlingTimer = 0f;
    private int currentClipIndex = 0;
    private bool audioStarted = false;
    private bool talkingSet = false;

    private Transform player;
    private bool approaching;
    private bool circling;
    private float circleAngle;
    private float fixedY;

    public DogJumpMimic jumpMimic;

    public void StartEncounter(Transform playerTransform)
    {
        player = playerTransform;
        approaching = true;
        circling = false;
        circleAngle = 0f;
        circlingTimer = 0f;
        currentClipIndex = 0;
        audioStarted = false;
        talkingSet = false;
        fixedY = transform.position.y;

        animator.SetBool("isMoving", true);
        animator.SetBool("isCircling", false);
        animator.SetBool("isTalking", false);

        if (playerController != null)
            playerController.canMove = false;
    }

    void Update()
    {
        if (approaching)
            MoveTowardsCircleEntry();

        if (circling)
            CircleAroundPlayer();
    }

    void MoveTowardsCircleEntry()
    {
        Vector3 toDog = (transform.position - player.position).normalized;
        Vector3 entryPoint = player.position + toDog * circleRadius;
        entryPoint.y = fixedY;

        if (Vector3.Distance(transform.position, entryPoint) > 0.05f)
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
            circlingTimer = 0f;
        }
    }

    void CircleAroundPlayer()
    {
        float dir = clockwise ? 1f : -1f;
        circleAngle += circleSpeed * Time.deltaTime * dir;

        float rad = circleAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * circleRadius;
        Vector3 targetPos = player.position + offset;
        targetPos.y = fixedY;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        RotateTowards((targetPos - transform.position).normalized);

        circlingTimer += Time.deltaTime;

        if (!talkingSet && circlingTimer >= talkingDelay)
        {
            animator.SetBool("isTalking", true);
            talkingSet = true;
        }

        if (!audioStarted && circlingTimer >= audioDelay && circlingClips.Length > 0)
        {
            audioSource.clip = circlingClips[0];
            audioSource.Play();
            audioStarted = true;
        }

        if (audioStarted && !audioSource.isPlaying)
        {
            currentClipIndex++;

            if (currentClipIndex < circlingClips.Length)
            {
                audioSource.clip = circlingClips[currentClipIndex];
                audioSource.Play();
            }
            else
            {
                StopCircling();
            }
        }
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
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

        if (jumpMimic != null)
            jumpMimic.enabled = true;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
