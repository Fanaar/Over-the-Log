using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class DogController : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1.5f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundOffset = 0.2f;

    [Header("Scene Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;
    public string sceneToLoad = "CaughtScene";

    private Transform target;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake() => controller = GetComponent<CharacterController>();

    void Start()
    {
        StartCoroutine(DelayedSnap());
    }

    void Update()
    {
        // Always run gravity so the controller stays grounded correctly even before chasing
        HandleGravity();

        if (isChasing && target != null)
            HandleMovement();
    }

    private void HandleMovement()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            controller.Move(dir.normalized * moveSpeed * Time.deltaTime);

            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (!hasCaughtPlayer)
        {
            hasCaughtPlayer = true;
            Debug.Log("🐶 Dog reached player!");
            StartCoroutine(FadeAndLoad());
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded)
            velocity.y = -groundOffset;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    public void StartChase(Transform chaseTarget)
    {
        target = chaseTarget;
        isChasing = true;
        hasCaughtPlayer = false;
        Debug.Log("🐶 Dog starts chasing " + chaseTarget.name);
    }

    public void StopChase() => isChasing = false;

    public void ForceGroundSnap()
    {
        if (controller == null) return;

        controller.enabled = false;

        Vector3 rayStart = transform.position + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 50f))
        {
            Vector3 pos = transform.position;
            // Correct formula: place the capsule center so the bottom touches the ground
            pos.y = hit.point.y + groundOffset + controller.center.y;
            transform.position = pos;

            // Reset X rotation
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }
        else
        {
            Debug.LogWarning("DogController: No ground detected under spawn point!");
        }

        controller.enabled = true;
    }

    private IEnumerator FadeAndLoad()
    {
        float timer = 0f;

        // If there's an Image on the canvas group, make sure its alpha can be driven
        if (fadeCanvasGroup != null && fadeCanvasGroup.GetComponent<UnityEngine.UI.Image>() != null)
        {
            var img = fadeCanvasGroup.GetComponent<UnityEngine.UI.Image>();
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator DelayedSnap()
    {
        yield return null;     // wait 1 frame
        yield return null;     // wait 2 frames (let gravity settle)
        ForceGroundSnap();     // NOW snap perfectly
    }
}
