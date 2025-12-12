using UnityEngine;

public class RunAwayRabbitController : MonoBehaviour
{
    [Header("Direction Handle")]
    public Transform directionHandle;
    public float speed = 5f;

    private Vector3 runDirection;

    void OnEnable()
    {
        // Bereken runDirection zodra het konijn geactiveerd wordt
        if (directionHandle != null)
        {
            Vector3 dir = directionHandle.position - transform.position;
            dir.y = 0; // alleen XZ-plane
            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward; // fallback

            runDirection = dir.normalized;
        }
        else
        {
            runDirection = transform.forward;
        }
    }

    void Update()
    {
        if (runDirection.sqrMagnitude < 0.001f)
            return;

        // Beweeg
        transform.position += runDirection * speed * Time.deltaTime;

        // Draai
        transform.rotation = Quaternion.LookRotation(runDirection);
    }
}
