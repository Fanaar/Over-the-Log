using UnityEngine;
using System.Collections;

public class RabbitCircleBeatRotator : MonoBehaviour
{
    [Header("Beat Settings")]
    public float bpm = 120f;                // Beats per minute
    public float rotationPerBeat = 30f;     // Degrees to rotate each beat
    public float rotationDuration = 0.2f;   // Duration of the rotation animation

    private float beatInterval;
    private bool isRotating = false;

    void Start()
    {
        beatInterval = 60f / bpm;           // convert BPM to seconds per beat
        StartCoroutine(BeatRotationCoroutine());
    }

    IEnumerator BeatRotationCoroutine()
    {
        while (true)
        {
            // Rotate smoothly on beat
            yield return StartCoroutine(RotateByDegrees(rotationPerBeat, rotationDuration));

            // Wait until next beat
            float waitTime = beatInterval - rotationDuration;
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator RotateByDegrees(float degrees, float duration)
    {
        if (duration <= 0f)
        {
            transform.Rotate(Vector3.up, degrees);
            yield break;
        }

        float elapsed = 0f;
        float startRotation = transform.eulerAngles.y;
        float targetRotation = startRotation + degrees;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float yRotation = Mathf.Lerp(startRotation, targetRotation, elapsed / duration);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
            yield return null;
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetRotation, transform.eulerAngles.z);
    }
}
