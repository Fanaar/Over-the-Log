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
    public float groundCheckDistance = 5f;

    [Header("Scene Fade Settings")]
    public CanvasGroup fadeCanvasGroup;   // assign your fade panel here
    public float fadeDuration = 1.5f;
    public string sceneToLoad = "CaughtScene";

    private Transform target;
    private bool isChasing = false;
    private bool hasCaughtPlayer = false; // prevent multiple triggers

    void Update()
    {
        if (!isChasing || target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        float distance = dir.magnitude;

        if (distance > stopDistance)
        {
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;

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

        StickToGround();
    }

    private void StickToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + groundOffset;
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

        // make sure image is visible
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
