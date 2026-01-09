using UnityEngine;

[RequireComponent(typeof(StressManager))]
public class SkyboxController : MonoBehaviour
{
    [Header("Base Skybox Values")]
    [Tooltip("Rustige / neutrale staat waar alles altijd naartoe kan teruglerpen")]
    public float baseAtmosphereThickness = 1.2f;
    public float baseExposure = 1.0f;
    public Color baseSkyTint = Color.white;
    public Color baseGroundTint = Color.white;

    [Header("Stress Influence")]
    [Range(0f, 5f)] public float stressAtmosphereOffset = -0.6f;
    [Range(0f, 8f)] public float stressExposureOffset = -0.5f;
    public Color stressSkyTint = new Color(0.6f, 0.2f, 0.2f);
    public Color stressGroundTint = Color.gray;

    [Header("Acceptance Influence")]
    [Range(0f, 5f)] public float acceptanceAtmosphereOffset = 2.1f;
    [Range(0f, 5f)] public float acceptanceExposureOffset = 2.5f;
    public Color acceptanceSkyTint = new Color(0.6f, 0.9f, 1f);
    public Color acceptanceGroundTint = Color.white;

    [Header("Blend Settings")]
    public float blendSpeed = 1.2f;

    private StressManager stressManager;
    private Material skyboxMat;

    private void Awake()
    {
        stressManager = GetComponent<StressManager>();
        skyboxMat = RenderSettings.skybox;

        if (skyboxMat == null)
            Debug.LogWarning("[SkyboxController] Geen skybox material gevonden!");
    }

    private void Update()
    {
        if (skyboxMat == null || stressManager == null) return;

        float stress01 = stressManager.stress / 100f;
        float acceptance01 = stressManager.Acceptance01;

        // ===== TARGET VALUES (BASE + OFFSETS) =====

        float targetAtmosphere =
            baseAtmosphereThickness
            + stressAtmosphereOffset * stress01
            + acceptanceAtmosphereOffset * acceptance01;

        float targetExposure =
            baseExposure
            + stressExposureOffset * stress01
            + acceptanceExposureOffset * acceptance01;

        Color targetSkyTint =
            Color.Lerp(baseSkyTint, stressSkyTint, stress01);
        targetSkyTint =
            Color.Lerp(targetSkyTint, acceptanceSkyTint, acceptance01);

        Color targetGroundTint =
            Color.Lerp(baseGroundTint, stressGroundTint, stress01);
        targetGroundTint =
            Color.Lerp(targetGroundTint, acceptanceGroundTint, acceptance01);

        // ===== SMOOTH BLEND =====

        skyboxMat.SetFloat(
            "_AtmosphereThickness",
            Mathf.Lerp(
                skyboxMat.GetFloat("_AtmosphereThickness"),
                targetAtmosphere,
                Time.deltaTime * blendSpeed
            )
        );

        skyboxMat.SetFloat(
            "_Exposure",
            Mathf.Lerp(
                skyboxMat.GetFloat("_Exposure"),
                targetExposure,
                Time.deltaTime * blendSpeed
            )
        );

        skyboxMat.SetColor(
            "_SkyTint",
            Color.Lerp(
                skyboxMat.GetColor("_SkyTint"),
                targetSkyTint,
                Time.deltaTime * blendSpeed
            )
        );

        skyboxMat.SetColor(
            "_GroundColor",
            Color.Lerp(
                skyboxMat.GetColor("_GroundColor"),
                targetGroundTint,
                Time.deltaTime * blendSpeed
            )
        );
    }
}
