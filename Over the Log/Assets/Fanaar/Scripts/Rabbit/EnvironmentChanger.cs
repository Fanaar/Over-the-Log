using UnityEngine;

public class SkyboxAndFogFader : MonoBehaviour
{
    [Header("Skybox Transition")]
    public Material skyboxMaterial; // The procedural skybox material
    public float targetExposure = 1f;
    public float targetAtmosphere = 0.4f;

    [Header("Fog Transition")]
    public bool enableFog = true;
    public Color targetFogColor = Color.gray;
    public float targetFogDensity = 0.01f;

    public float transitionDuration = 2f;

    // Initial values
    private float initialExposure;
    private float initialAtmosphere;

    private Color initialFogColor;
    private float initialFogDensity;
    private bool initialFogEnabled;

    private float t = 0f;
    private bool isFading = false;

    private void OnEnable()
    {
        if (skyboxMaterial == null) return;

        // Store initial skybox values
        initialExposure = skyboxMaterial.GetFloat("_Exposure");
        initialAtmosphere = skyboxMaterial.GetFloat("_AtmosphereThickness");

        // Store initial fog values
        initialFogColor = RenderSettings.fogColor;
        initialFogDensity = RenderSettings.fogDensity;
        initialFogEnabled = RenderSettings.fog;

        t = 0f;
        isFading = true;
    }

    private void Update()
    {
        if (!isFading) return;

        t += Time.deltaTime / transitionDuration;
        float clampedT = Mathf.Clamp01(t);

        // Lerp skybox values
        skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(initialExposure, targetExposure, clampedT));
        skyboxMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(initialAtmosphere, targetAtmosphere, clampedT));

        // Lerp fog values
        RenderSettings.fog = enableFog;
        RenderSettings.fogColor = Color.Lerp(initialFogColor, targetFogColor, clampedT);
        RenderSettings.fogDensity = Mathf.Lerp(initialFogDensity, targetFogDensity, clampedT);

        if (clampedT >= 1f)
        {
            isFading = false;
        }
    }
}
