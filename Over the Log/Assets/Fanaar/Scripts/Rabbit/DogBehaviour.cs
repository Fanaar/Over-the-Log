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
    public DogJumpMimic jumpMimic;

    [Header("Circling Voice Audio")]
    public AudioSource voiceSource;
    public AudioClip[] circlingClips;
    public float audioDelay = 2f;
    public float talkingDelay = 2f;

    [Header("Emotional Music")]
    public AudioSource emotionalMusicSource;
    public AudioClip emotionalMusicClip;
    public float emotionalMusicFadeDuration = 4f;
    [Range(0f, 1f)] public float emotionalMusicTargetVolume = 1f;
    public GameObject objectToActivateAfterMusic;

    [Header("Falling Flowers")]
    public GameObject fallingFlowers;
    public float flowersDelay = 3f;

    [Header("Post Processing")]
    public SimplePostProcessingWithDarkBorders postProcessing;
    private bool postProcessingTriggered = false;


    // ──────────────────────────────
    // Internal state
    // ──────────────────────────────

    private Transform player;

    private bool approaching;
    private bool circling;
    private bool finalApproach;

    private float circleAngle;
    private float fixedY;
    private float finalApproachTimer;
    private float circlingTimer;

    private bool talkingSet = false;
    private bool audioStarted = false;
    private int currentClipIndex = 0;

    private bool emotionalMusicStarted = false;
    private bool emotionalMusicFinished = false;
    private float emotionalMusicFadeTimer = 0f;

    private bool flowersActivated = false;

    // ──────────────────────────────
    // Public entry
    // ──────────────────────────────

    public void StartEncounter(Transform playerTransform)
    {
        player = playerTransform;
        postProcessingTriggered = false;

        approaching = true;
        circling = false;
        finalApproach = false;

        circleAngle = 0f;
        finalApproachTimer = 0f;
        circlingTimer = 0f;

        talkingSet = false;
        audioStarted = false;
        currentClipIndex = 0;

        emotionalMusicStarted = false;
        emotionalMusicFinished = false;
        emotionalMusicFadeTimer = 0f;

        flowersActivated = false;

        fixedY = transform.position.y;

        animator.SetBool("isMoving", true);
        animator.SetBool("isCircling", false);
        animator.SetBool("isTalking", false);

        if (fallingFlowers != null)
            fallingFlowers.SetActive(false);

        if (playerController != null)
            playerController.canMove = false;
    }

    // ──────────────────────────────
    // Update
    // ──────────────────────────────

    void Update()
    {
        if (approaching)
            MoveTowardsCircleEntry();

        if (circling)
            CircleAroundPlayer();

        if (finalApproach)
            FinalApproach();

        HandleEmotionalMusicFade();
        CheckEmotionalMusicFinished();
        HandleFallingFlowers();
    }

    // ──────────────────────────────
    // Movement logic
    // ──────────────────────────────

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

            animator.SetBool("isMoving", false);
            animator.SetBool("isCircling", true);

            circlingTimer = 0f;
            talkingSet = false;
            audioStarted = false;
            currentClipIndex = 0;
        }
    }

    void CircleAroundPlayer()
    {
        // Start emotional music (once)
        if (!emotionalMusicStarted && emotionalMusicClip != null && emotionalMusicSource != null)
        {
            emotionalMusicSource.volume = 0f;
            emotionalMusicSource.clip = emotionalMusicClip;
            emotionalMusicSource.Play();

            emotionalMusicFadeTimer = 0f;
            emotionalMusicStarted = true;
        }

        float dir = clockwise ? 1f : -1f;
        circleAngle += circleSpeed * Time.deltaTime * dir;

        float rad = circleAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * circleRadius;
        Vector3 targetPos = player.position + offset;
        targetPos.y = fixedY;

        Vector3 moveDir = (targetPos - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        RotateTowards(moveDir);

        circlingTimer += Time.deltaTime;

        // Talking animation
        if (!talkingSet && circlingTimer >= talkingDelay)
        {
            animator.SetBool("isTalking", true);
            talkingSet = true;
        }

        // Voice clips
        if (!audioStarted && circlingTimer >= audioDelay && circlingClips.Length > 0 && voiceSource != null)
        {
            voiceSource.clip = circlingClips[currentClipIndex];
            voiceSource.Play();
            audioStarted = true;
        }

        if (audioStarted && voiceSource != null && !voiceSource.isPlaying)
        {
            currentClipIndex++;

            if (currentClipIndex < circlingClips.Length)
            {
                voiceSource.clip = circlingClips[currentClipIndex];
                voiceSource.Play();
            }
            else
            {
                TriggerPostProcessing();
                StartFinalApproach();
            }
        }
    }

    void StartFinalApproach()
    {
        circling = false;
        finalApproach = true;
        finalApproachTimer = 0f;

        animator.SetBool("isCircling", false);
        animator.SetBool("isMoving", true);
        animator.SetBool("isTalking", false);
    }

    void TriggerPostProcessing()
    {
        if (postProcessingTriggered || postProcessing == null)
            return;

        postProcessingTriggered = true;
        postProcessing.FadeThirdVolumeIn();
    }

    void FinalApproach()
    {
        finalApproachTimer += Time.deltaTime;

        Vector3 targetPos = player.position;
        targetPos.y = fixedY;

        float dist = Vector3.Distance(transform.position, targetPos);

        if (dist <= stopDistance || finalApproachTimer >= finalApproachDuration)
        {
            FinishEncounter();
            return;
        }

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        RotateTowards(dir);
    }

    // ──────────────────────────────
    // Emotional music logic
    // ──────────────────────────────

    void HandleEmotionalMusicFade()
    {
        if (!emotionalMusicStarted || emotionalMusicFinished || emotionalMusicSource == null)
            return;

        if (emotionalMusicFadeTimer < emotionalMusicFadeDuration)
        {
            emotionalMusicFadeTimer += Time.deltaTime;
            float t = emotionalMusicFadeTimer / emotionalMusicFadeDuration;
            emotionalMusicSource.volume =
                Mathf.Lerp(0f, emotionalMusicTargetVolume, t);
        }
    }

    void CheckEmotionalMusicFinished()
    {
        if (emotionalMusicStarted && !emotionalMusicFinished && emotionalMusicSource != null)
        {
            if (!emotionalMusicSource.isPlaying)
            {
                emotionalMusicFinished = true;

                if (objectToActivateAfterMusic != null)
                    objectToActivateAfterMusic.SetActive(true);
            }
        }
    }

    // ──────────────────────────────
    // Flowers
    // ──────────────────────────────

    void HandleFallingFlowers()
    {
        if (!circling || flowersActivated || fallingFlowers == null)
            return;

        if (circlingTimer >= flowersDelay)
        {
            fallingFlowers.SetActive(true);
            flowersActivated = true;
        }
    }

    // ──────────────────────────────
    // Finish
    // ──────────────────────────────

    void FinishEncounter()
    {
        approaching = false;
        circling = false;
        finalApproach = false;

        animator.SetBool("isMoving", false);
        animator.SetBool("isCircling", false);
        animator.SetBool("isTalking", false);

        if (playerController != null)
            playerController.canMove = true;

        if (jumpMimic != null)
            jumpMimic.enabled = true;

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation =
            Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 6f);
    }
}
