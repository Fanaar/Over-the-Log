using UnityEngine;
using UnityEngine.SceneManagement;

public class StressManager : MonoBehaviour
{
    public static StressManager Instance;

    [Range(0, 100)]
    public float stress = 0f;

    [Header("Rates")]
    public float fleeRate = 15f;
    public float restlessRate = 5f;
    public float avoidRate = 8f;
    public float calmRate = 10f;

    [Header("Acceptance Settings")]
    public float acceptanceTime = 10f; // seconden kijken en stil
    private float acceptanceTimer = 0f;
    public string nextSceneName; // hier vul je in de Inspector de scene naam in

    [HideInInspector] public bool dogIsActive;
    [HideInInspector] public bool playerIsMoving;
    [HideInInspector] public bool playerIsSprinting;
    [HideInInspector] public bool playerIsLookingAtDog;
    [HideInInspector] public bool inDogTrigger = false;

    public FirstPersonRabbitController playerController;
    public DogController2 dog;

    // ----- Read-only property voor andere scripts -----
    public float Acceptance01 => Mathf.Clamp01(acceptanceTimer / acceptanceTime);

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!dogIsActive) return;

        // ----- Stress berekening -----
        if (playerIsSprinting)
            stress += fleeRate * Time.deltaTime * 1.5f;

        if (playerIsMoving)
            stress += restlessRate * Time.deltaTime;

        if (!playerIsLookingAtDog)
            stress += avoidRate * Time.deltaTime;

        // Stress dalen alleen als speler stil staat en kijkt naar hond
        if (playerIsLookingAtDog && inDogTrigger && !playerIsMoving)
            stress -= calmRate * Time.deltaTime;

        stress = Mathf.Clamp(stress, 0f, 100f);

        // ----- Acceptance -----
        bool isStill = !playerController.isMoving;

        if (playerIsLookingAtDog && isStill)
        {
            acceptanceTimer += Time.deltaTime;

            if (acceptanceTimer >= acceptanceTime)
            {
                Debug.Log("Acceptance complete! Laad volgende scene...");
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
                else
                    Debug.LogWarning("Next scene name niet ingesteld in de Inspector!");
            }
        }
        else
        {
            acceptanceTimer = 0f;
        }

        // Debug
        Debug.Log($"[StressManager] Stress: {stress:F1} | AcceptanceTimer: {acceptanceTimer:F1}");
    }
}
