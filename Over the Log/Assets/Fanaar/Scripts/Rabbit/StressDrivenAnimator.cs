using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StressDrivenAnimator : MonoBehaviour
{
    [Header("References")]
    public StressManager stressManager;
    public string animationStateName = "Spike circle animation";

    [Header("Animation Speeds")]
    public float forwardSpeed = 3f;

    [Header("Position Lerp")]
    public float acceptanceYOffset = -1.5f;
    public float positionLerpSpeed = 3f;

    public bool debugLog = false;

    private Animator animator;
    private Vector3 startLocalPosition;
    private Vector3 acceptanceLocalPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (stressManager == null)
            stressManager = StressManager.Instance;
    }

    void Start()
    {
        // 🔒 Animatie starten aan BEGIN
        animator.speed = 0f;
        animator.Play(animationStateName, 0, 0f);
        animator.Update(0f);

        // 📍 Posities opslaan
        startLocalPosition = transform.localPosition;
        acceptanceLocalPosition = startLocalPosition + Vector3.up * acceptanceYOffset;

        if (debugLog)
            Debug.Log("[StressDrivenAnimator] Initialized");
    }

    void Update()
    {
        if (stressManager == null) return;

        bool hasStress = stressManager.stress > 0f;
        bool acceptanceActive = stressManager.Acceptance01 > 0f;

        // 🌿 ACCEPTANCE → animatie pauze + naar beneden
        if (acceptanceActive)
        {
            animator.speed = 0f;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                acceptanceLocalPosition,
                Time.deltaTime * positionLerpSpeed
            );

            if (debugLog)
                Debug.Log("ACCEPTANCE → lowering object");
        }
        // 🔥 STRESS → animatie vooruit + terug omhoog
        else if (hasStress)
        {
            animator.speed = forwardSpeed;

            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                startLocalPosition,
                Time.deltaTime * positionLerpSpeed
            );

            if (debugLog)
                Debug.Log("STRESS → raising object");
        }
        // ⏸️ Niets actief → animatie pauze
        else
        {
            animator.speed = 0f;
        }
    }
}
