using UnityEngine;
using System.Collections;

public class DogJumpMimic : MonoBehaviour
{
    public float jumpForce = 4f;
    public float mimicDelay = 0.4f;

    private Rigidbody rb;

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
        StartCoroutine(DelayedJump());
    }

    IEnumerator DelayedJump()
    {
        yield return new WaitForSeconds(mimicDelay);

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
