using UnityEngine;
using System.Collections;

public class DogJumpMimic : MonoBehaviour
{
    public float jumpForce = 4f;
    public float mimicDelay = 0.4f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.25f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enabled = false; // pas actief na encounter
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
        isGrounded = Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void OnPlayerJump()
    {
        if (!isGrounded) return;

        if (rb != null)
            rb.isKinematic = false;

        StartCoroutine(DelayedJump());
    }

    IEnumerator DelayedJump()
    {
        yield return new WaitForSeconds(mimicDelay);

        if (rb != null && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
