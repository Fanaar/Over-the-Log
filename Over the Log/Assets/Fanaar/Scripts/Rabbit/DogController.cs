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
    public float gravity = -9.81f; // downward force
    public float groundOffset = 0.2f; // offset from terrain

    [Header("Scene Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;
    public string sceneToLoad = "CaughtScene";

    private Transform target;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false;

    private CharacterController controller;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        ForceGroundSnap();
    }


    void Update()
    {
        if (!isChasing || target == null) return;

        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            // Move using CharacterController
            Vector3 move = dir.normalized * moveSpeed;
            controller.Move(move * Time.deltaTime);

            // Rotate smoothly toward target
            if (dir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (!hasCaughtPlayer)
            {
                hasCaughtPlayer = true;
                Debug.Log("🐶 Dog reached player!");
                StartCoroutine(FadeAndLoad());
            }
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded)
        {
            // Stick slightly above ground to avoid sinking
            velocity.y = -groundOffset;
        }
        else
        {
            // Apply gravity when in the air
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    public void StartChase(Transform chaseTarget)
    {
        target = chaseTarget;
        isChasing = true;
        hasCaughtPlayer = false;
        Debug.Log("🐶 Dog starts chasing " + target.name);
    }

    public void StopChase()
    {
        isChasing = false;
    }

    private System.Collections.IEnumerator FadeAndLoad()
    {
        float timer = 0f;

        if (fadeCanvasGroup.GetComponent<UnityEngine.UI.Image>() != null)
        {
            var img = fadeCanvasGroup.GetComponent<UnityEngine.UI.Image>();
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        fadeCanvasGroup.alpha = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
    public void ForceGroundSnap()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller == null) return;

        controller.enabled = false; // temporarily disable

        // raycast to ground
        Vector3 rayStart = transform.position + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 50f))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + groundOffset + (controller.height / 2f) - controller.center.y;
            transform.position = pos;

            // reset X rotation
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        controller.enabled = true; // re-enable
    }

}

