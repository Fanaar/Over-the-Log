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
    public AudioSource audioSource;           // AudioSource voor de hond
    public AudioClip circlingClip;            // Clip die afgespeeld wordt tijdens cirkelen
    public float audioDelay = 2f;             // Tijd (in seconden) voordat audio start
    private bool audioPlayed = false;         // Check of audio al is afgespeeld
    private float circlingTimer = 0f;         // Timer voor audio delay

    private Transform player;
    private bool approaching;
    private bool circling;
    private bool finalApproach;
    private float circleAngle;
    private float fixedY;
    private float finalApproachTimer;

    public DogJumpMimic jumpMimic;

    public void StartEncounter(Transform playerTransform)
    {
        player = playerTransform;
        approaching = true;
        circling = false;
        finalApproach = false;
        circleAngle = 0f;
        finalApproachTimer = 0f;
        circlingTimer = 0f;
        audioPlayed = false;
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

        if (finalApproach)
            FinalApproach();

        if (circling && Input.GetKeyDown(KeyCode.T))
            StartFinalApproach();
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
            circlingTimer = 0f;
            audioPlayed = false;
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

        Vector3 moveDir = (targetPos - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        RotateTowards(moveDir);

        animator.SetBool("isMoving", true);

        // --- Audio timer ---
        if (!audioPlayed)
        {
            circlingTimer += Time.deltaTime;
            if (circlingTimer >= audioDelay && audioSource != null && circlingClip != null)
            {
                audioSource.clip = circlingClip;
                audioSource.Play();
                audioPlayed = true;
            }
        }

        // --- Talking bool na 2 seconden ---
        if (circlingTimer >= 2f)  // Zet na 2 seconden
        {
            animator.SetBool("isTalking", true);
        }
    }



    void StartFinalApproach()
    {
        circling = false;
        finalApproach = true;
        finalApproachTimer = 0f;

        animator.SetBool("isCircling", false);
        animator.SetBool("isMoving", true);
    }

    void FinalApproach()
    {
        finalApproachTimer += Time.deltaTime;

        Vector3 targetPos = player.position;
        targetPos.y = fixedY;

        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist <= stopDistance)
        {
            StopCircling();
            return;
        }

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        RotateTowards(dir);

        if (finalApproachTimer >= finalApproachDuration)
            StopCircling();
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
        finalApproach = false;

        animator.SetBool("isCircling", false);
        animator.SetBool("isMoving", false);
        animator.SetBool("isTalking", false);

        if (playerController != null)
            playerController.canMove = true;

        if (jumpMimic != null)
            jumpMimic.enabled = true;

        // Optioneel: stop audio als het nog speelt
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void SetTalking(bool talking)
    {
        animator.SetBool("isTalking", talking);
    }
}
