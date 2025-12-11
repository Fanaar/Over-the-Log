using UnityEngine;
using UnityEngine.Rendering;

public class PostProcessTransition : MonoBehaviour
{
    public Volume underTheTreesVolume;
    public Volume aboveTheTreesVolume;
    public float transitionSpeed = 1f;
    public bool toggleFog = true; // Do you want to control fog?

    private bool hasTransitioned = false;
    private float t = 0f;

    private void Update()
    {
        if (hasTransitioned && t < 1f)
        {
            t = Mathf.MoveTowards(t, 1f, Time.deltaTime * transitionSpeed);

            underTheTreesVolume.weight = 1f - t;
            aboveTheTreesVolume.weight = t;

            // Optional: Fade out fog along with transition
            if (toggleFog)
            {
                RenderSettings.fog = true; // Ensure it's on initially
                RenderSettings.fogDensity = Mathf.Lerp(0.02f, 0f, t); // Adjust start/end density
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTransitioned)
        {
            hasTransitioned = true;
        }
    }
}
