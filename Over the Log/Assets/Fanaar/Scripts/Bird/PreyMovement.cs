using UnityEngine;

public class PreyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float moveDistance = 5f;

    private Vector3 startPos;
    private bool isCaught = false;
    private float direction = 1f;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (isCaught)
            return;

        // Simple left-right movement
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        // Flip direction at range limits
        if (Vector3.Distance(startPos, transform.position) >= moveDistance)
            direction *= -1f;
    }

    // ✅ Called when player wants the prey to stop soon
    public void PrepareForStop(float delay = 0.5f)
    {
        StartCoroutine(StopAfterDelay(delay));
    }

    private System.Collections.IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnCaught();
    }

    // ✅ Now callable from other scripts
    public void OnCaught()
    {
        isCaught = true;
        Debug.Log("🛑 Prey stopped moving!");
    }
}
