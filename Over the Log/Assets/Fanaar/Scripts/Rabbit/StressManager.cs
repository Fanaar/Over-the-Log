using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;


public class StressManager : MonoBehaviour
{
    public static StressManager Instance;

    [Range(0, 100)]
    public float stress = 0f;

    [Header("Rates")]
    public float fleeRate = 15f;      // sprinten
    public float restlessRate = 5f;   // bewegen zonder te accepteren
    public float avoidRate = 8f;      // wegkijken
    public float calmRate = 10f;      // kalmeren

    [Header("Acceptance Settings")]
    public float acceptanceTime = 10f;
    private float acceptanceTimer = 0f;
    public string nextSceneName;

    [Header("FMOD")]
    public StudioEventEmitter[] emittersToStop;


    [HideInInspector] public bool dogIsActive;
    [HideInInspector] public bool playerIsMoving;
    [HideInInspector] public bool playerIsSprinting;
    [HideInInspector] public bool playerIsLookingAtDog;
    [HideInInspector] public bool inDogTrigger = false;

    public FirstPersonRabbitController playerController;
    public DogController2 dog;

    // ----- Read-only properties -----
    public float Acceptance01 => Mathf.Clamp01(acceptanceTimer / acceptanceTime);
    public bool AcceptanceSitReached => acceptanceTimer >= 5f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!dogIsActive) return;

        HandleStress();
        HandleAcceptance();

        // Debug
        Debug.Log($"[StressManager] Stress: {stress:F1} | AcceptanceTimer: {acceptanceTimer:F1}");
    }

    // ================================
    // STRESS
    // ================================
    void HandleStress()
    {
        // Sprinten = paniek
        if (playerIsSprinting)
            stress += fleeRate * Time.deltaTime * 1.5f;

        // Normaal bewegen zonder acceptatie
        if (playerIsMoving && !playerIsLookingAtDog)
            stress += restlessRate * Time.deltaTime;

        // Wegkijken
        if (!playerIsLookingAtDog)
            stress += avoidRate * Time.deltaTime;

        // Kalmeren zolang je kijkt (ook tijdens lopen, maar niet sprinten)
        if (playerIsLookingAtDog && inDogTrigger && !playerIsSprinting)
            stress -= calmRate * Time.deltaTime;

        stress = Mathf.Clamp(stress, 0f, 100f);
    }

    // ================================
    // ACCEPTANCE
    // ================================
    void HandleAcceptance()
    {
        bool canAccept =
            playerIsLookingAtDog &&
            inDogTrigger &&
            !playerIsSprinting;

        if (canAccept)
        {
            acceptanceTimer += Time.deltaTime;

            if (acceptanceTimer >= acceptanceTime)
            {
                Debug.Log("Acceptance complete! Laad volgende scene...");

                StopFMODEmittersImmediate();

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
    }



    // ================================
    // FMOD
    // ================================
    void StopFMODEmittersImmediate()
    {
        if (emittersToStop == null) return;

        foreach (var emitter in emittersToStop)
        {
            if (emitter == null) continue;

            emitter.EventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            emitter.EventInstance.release();
        }
    }

}
