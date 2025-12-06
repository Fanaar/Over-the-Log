using UnityEngine;
using UnityEngine.SceneManagement;

public class DogController : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 5f;
    public float stopDistance = 1.5f;

    [Header("Grounding")]
    public float groundOffset = 0.2f;
    public float groundCheckDistance = 2f;
    public float groundStickSpeed = 10f; // how fast the dog adjusts to ground height
    public float groundCheckRadius = 0.5f; // radius for SphereCast

    [Header("Scene Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.5f;
    public string sceneToLoad = "CaughtScene";

    private Transform target;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false;

    void Update()
    {
        if (!isChasing || target == null) return;

        HandleMovement();
        StickToGround();
    }

    private void HandleMovement()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            // Smooth horizontal movement
            Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
            transform.position += move;

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

    public void StickToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(rayOrigin, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            Vector3 pos = transform.position;
            float targetY = hit.point.y + groundOffset;
            pos.y = Mathf.Lerp(pos.y, targetY, groundStickSpeed * Time.deltaTime); // smooth vertical movement
            transform.position = pos;
        }
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
}
