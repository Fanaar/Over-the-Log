using UnityEngine;
using Unity.Cinemachine;

public class CameraZoneSwitcher : MonoBehaviour
{
    public CinemachineCamera vcamUnderTrees;
    public CinemachineCamera vcamAboveTrees;

    public int activePriority = 20;
    public int inactivePriority = 10;

    [Header("Fog Settings (Lighting tab fog)")]
    public float fogUnderTrees = 0.02f;   // jouw huidige waarde
    public float fogAboveTrees = 0.0f;    // bv. minder mist boven de bomen
    public float fogFadeDuration = 1f;

    private Coroutine fogRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        vcamUnderTrees.Priority = inactivePriority;
        vcamAboveTrees.Priority = activePriority;

        // Mist fade van UNDER → ABOVE
        StartFogFade(fogUnderTrees, fogAboveTrees);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        vcamUnderTrees.Priority = activePriority;
        vcamAboveTrees.Priority = inactivePriority;

        // Mist fade van ABOVE → UNDER
        StartFogFade(fogAboveTrees, fogUnderTrees);
    }

    private void StartFogFade(float from, float to)
    {
        if (fogRoutine != null)
            StopCoroutine(fogRoutine);

        fogRoutine = StartCoroutine(FogFadeRoutine(from, to));
    }

    private System.Collections.IEnumerator FogFadeRoutine(float start, float end)
    {
        float t = 0f;

        while (t < fogFadeDuration)
        {
            float n = t / fogFadeDuration;
            RenderSettings.fogDensity = Mathf.Lerp(start, end, n);

            t += Time.deltaTime;
            yield return null;
        }

        RenderSettings.fogDensity = end;
    }
}
