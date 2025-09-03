using UnityEngine;

public class SnakePatrol : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 3f;
    public float chaseSpeed = 5f;
    public float bodyFollowSpeed = 5f;
    public float wiggleAmplitude = 2f;
    public float wiggleFrequency = 3f;
    public float biteRadius = 0.5f;
    public float detectionRadius = 5f; // distance to start chasing player

    private int currentPoint = 0;
    private SnakeGenerator generator;
    private float wiggleOffset;
    private Transform player;
    private bool isChasing = false;

    void Start()
    {
        generator = GetComponent<SnakeGenerator>();
        wiggleOffset = Random.Range(0f, 2f * Mathf.PI);
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (generator == null || generator.bodySegments.Count == 0) return;
        Transform head = generator.bodySegments[0];

        // Check distance to player
        if (player != null)
        {
            float distance = Vector3.Distance(head.position, player.position);
            isChasing = distance <= detectionRadius;
        }

        // Target for movement
        Vector3 targetPos = isChasing && player != null ? player.position : patrolPoints[currentPoint].position;
        float moveSpeed = isChasing ? chaseSpeed : speed;

        // Move head
        Vector3 direction = (targetPos - head.position).normalized;
        head.position += direction * moveSpeed * Time.deltaTime;

        // Wriggle sideways
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        head.position += right * Mathf.Sin(Time.time * wiggleFrequency + wiggleOffset) * wiggleAmplitude * Time.deltaTime;

        head.LookAt(targetPos);

        // Patrol switching (only if not chasing)
        if (!isChasing && Vector3.Distance(head.position, patrolPoints[currentPoint].position) < 0.1f)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;

        // Body follows
        for (int i = 1; i < generator.bodySegments.Count; i++)
        {
            Transform prev = generator.bodySegments[i - 1];
            Transform current = generator.bodySegments[i];
            Vector3 followPos = prev.position - prev.forward * generator.spacing;
            current.position = Vector3.Lerp(current.position, followPos, bodyFollowSpeed * Time.deltaTime);
            current.LookAt(prev);
        }

        // Bite detection
        Collider[] hits = Physics.OverlapSphere(head.position, biteRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("You've been bitten!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (generator != null && generator.bodySegments.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(generator.bodySegments[0].position, biteRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(generator.bodySegments[0].position, detectionRadius);
        }
    }
}
