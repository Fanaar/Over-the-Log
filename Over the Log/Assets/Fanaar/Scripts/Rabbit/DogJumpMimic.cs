using UnityEngine;
using System.Collections;

public class DogJumpMimic : MonoBehaviour
{
    public float jumpForce = 4f;
    public float mimicDelay = 0.4f;

    [Header("Post Processing")]
    public SimplePostProcessingWithDarkBorders postProcessing;

    [Header("Jump Audio")]
    public AudioSource audioSource;         // AudioSource voor de hond
    public AudioClip specialJumpClip;       // Clip die afspeelt na X sprongen
    public int jumpAudioThreshold = 10;     // Hoeveel sprongen voordat special clip afspeelt

    private Rigidbody rb;
    private bool hasTriggeredPost = false;
    public GameObject fallingFlowers;

    private int jumpCount = 0;              // Telt het aantal sprongen

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

    void OnPlayerJump()
    {
        // Eerste keer post-processing triggeren
        if (!hasTriggeredPost && postProcessing != null)
        {
            hasTriggeredPost = true;
            postProcessing.FadeThirdVolumeIn();
        }

        // Falling leaves activeren
        if (fallingFlowers != null)
            fallingFlowers.SetActive(true);

        // Zet Rigidbody fysica aan (IsKinematic uit)
        if (rb != null)
            rb.isKinematic = false;

        // Tel sprong
        jumpCount++;

        // Speel audio als threshold bereikt
        if (audioSource != null && specialJumpClip != null && jumpCount >= jumpAudioThreshold)
        {
            audioSource.clip = specialJumpClip;
            audioSource.Play();
            jumpCount = 0; // reset teller als je wilt dat het opnieuw kan
        }

        // Start jump
        StartCoroutine(DelayedJump());
    }

    IEnumerator DelayedJump()
    {
        yield return new WaitForSeconds(mimicDelay);

        if (rb != null)
        {
            // reset verticale snelheid
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            // apply jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
