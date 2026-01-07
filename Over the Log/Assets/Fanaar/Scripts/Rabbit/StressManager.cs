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

    public FirstPersonRabbitController playerController; // nodig voor stillness check
    public DogController2 dog; // optioneel, alleen voor debug/log

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!dogIsActive) return;

        // ----- Stress berekenen -----
        if (playerIsSprinting)
            stress += fleeRate * Time.deltaTime * 1.5f;

        if (playerIsMoving)
            stress += restlessRate * Time.deltaTime;

        if (!playerIsLookingAtDog && !inDogTrigger)
            stress += avoidRate * Time.deltaTime;

        if (inDogTrigger && playerIsLookingAtDog)
            stress -= calmRate * Time.deltaTime;

        stress = Mathf.Clamp(stress, 0f, 100f);

        // ----- Acceptance logica -----
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
            acceptanceTimer = 0f; // reset timer als speler beweegt of niet kijkt
        }

        // Debug
        Debug.Log($"[DEBUG] Stress: {stress:F1} | AcceptanceTimer: {acceptanceTimer:F1}");
    }
}
