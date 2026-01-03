using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RabbitSettlementManager : MonoBehaviour
{
    public static RabbitSettlementManager Instance;

    [Header("Rabbits")]
    public int totalRabbits = 5;
    private HashSet<RabbitRunAwayController> settledRabbits = new HashSet<RabbitRunAwayController>();

    [Header("Burrow / Escape")]
    public float digTime = 30f;
    public GameObject[] blockingCubes;
    public AudioSource diggingAudio;
    public ParticleSystem diggingParticles;

    [Header("Dog")]
    public GameObject dog;
    private bool diggingStarted = false;

    // ⬇⬇ NIEUW (maar los van oude logica)
    [Header("After Burrow Opens")]
    [SerializeField] private GameObject[] rabbitsToDisable;
    [SerializeField] private GameObject[] holeTriggerObjects;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Hole triggers altijd eerst UIT
        foreach (var trigger in holeTriggerObjects)
        {
            if (trigger != null)
                trigger.SetActive(false);
        }
    }

    public void RabbitSettled(RabbitRunAwayController rabbit)
    {
        if (settledRabbits.Contains(rabbit))
            return;

        settledRabbits.Add(rabbit);
        Debug.Log($"Konijn gesettled! Totaal: {settledRabbits.Count}/{totalRabbits}");

        if (settledRabbits.Count == 3 && dog != null)
        {
            dog.SetActive(true);
            Debug.Log("Hond wordt geactiveerd bij 3e konijn!");
        }

        if (settledRabbits.Count >= totalRabbits && !diggingStarted)
        {
            StartCoroutine(DigBurrow());
        }
    }

    private IEnumerator DigBurrow()
    {
        diggingStarted = true;
        Debug.Log("Burrow graven gestart!");

        if (diggingAudio) diggingAudio.Play();
        if (diggingParticles) diggingParticles.Play();

        yield return new WaitForSeconds(digTime);

        foreach (GameObject cube in blockingCubes)
        {
            cube.SetActive(false);
        }

        Debug.Log("Burrow is nu open!");

        if (diggingAudio) diggingAudio.Stop();
        if (diggingParticles) diggingParticles.Stop();

        // ⬇⬇ HIER haak je je nieuwe gedrag aan
        OnBurrowOpened();
    }

    private void OnBurrowOpened()
    {
        // Konijntjes uit
        foreach (var rabbit in rabbitsToDisable)
        {
            if (rabbit != null)
                rabbit.SetActive(false);
        }

        // Hole triggers aan
        foreach (var trigger in holeTriggerObjects)
        {
            if (trigger != null)
                trigger.SetActive(true);
        }
    }
}
