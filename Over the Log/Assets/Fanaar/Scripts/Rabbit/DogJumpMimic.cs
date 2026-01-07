using UnityEngine;
using System.Collections;

public class DogJumpMimic : MonoBehaviour
{
    public float jumpForce = 4f;
    public float mimicDelay = 0.4f;

    [Header("Post Processing")]
    public SimplePostProcessingWithDarkBorders postProcessing;

    [Header("Jump Audio")]
    public AudioSource audioSource;
    public AudioClip specialJumpClip;
    public int jumpAudioThreshold = 10;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Activate Object After Jumps")]
    public GameObject objectToActivate;    // object dat geactiveerd wordt
    public int jumpActivateThreshold = 5;  // na hoeveel sprongen

    private Rigidbody rb;
    private bool hasTriggeredPost = false;
    private bool isGrounded = true;
    private bool hasPlayedMusic = false;
    private bool hasActivatedObject = false;  // voorkomt dat we het meerdere keren activeren
    public GameObject fallingFlowers;

    private int jumpCount = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enabled = false; // wordt pas aangezet na stop
    }

    void OnEnable()
    {
        FirstPersonRabbitController.OnPlayerJump += OnPlayerJump;
    }

    void OnDisable()
    {
        FirstPersonRabbitController.OnPlayerJump -= OnPlayerJump;
    }

    void Update()
    {
        // Ground check met CheckSphere
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    void OnPlayerJump()
    {
        // alleen springen als we grounded zijn
        if (!isGrounded) return;

        // Eerste keer post-processing triggeren
        if (!hasTriggeredPost && postProcessing != null)
        {
            hasTriggeredPost = true;
            postProcessing.FadeThirdVolumeIn();
        }

        // Falling flowers activeren
        if (fallingFlowers != null)
            fallingFlowers.SetActive(true);

        // Rigidbody fysica aanzetten
        if (rb != null)
            rb.isKinematic = false;

        // Tel sprong
        jumpCount++;

        // Speel audio als threshold bereikt, maar alleen 1 keer
        if (!hasPlayedMusic && audioSource != null && specialJumpClip != null && jumpCount >= jumpAudioThreshold)
        {
            audioSource.clip = specialJumpClip;
            audioSource.Play();
            hasPlayedMusic = true;
        }

        // Activeer object na bepaalde aantal sprongen
        if (!hasActivatedObject && objectToActivate != null && jumpCount >= jumpActivateThreshold)
        {
            objectToActivate.SetActive(true);
            hasActivatedObject = true;
        }

        // Start jump
        StartCoroutine(DelayedJump());
    }

    IEnumerator DelayedJump()
    {
        yield return new WaitForSeconds(mimicDelay);

        if (rb != null && isGrounded)
        {
            // reset verticale snelheid
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            // apply jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
