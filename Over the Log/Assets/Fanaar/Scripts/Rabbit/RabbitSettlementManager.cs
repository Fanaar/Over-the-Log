using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RabbitSettlementManager : MonoBehaviour
{
    public static RabbitSettlementManager Instance;

    [Header("Rabbits")]
    public int totalRabbits = 5;                        // Totaal aantal konijnen
    private HashSet<RabbitRunAwayController> settledRabbits = new HashSet<RabbitRunAwayController>();

    [Header("Burrow / Escape")]
    public float digTime = 30f;                         // Tijd voordat burrow open is
    public GameObject[] blockingCubes;                  // Cubes die de opening blokkeren
    public AudioSource diggingAudio;                    // Optionele audio
    public ParticleSystem diggingParticles;            // Optionele particle effect

    [Header("Dog")]
    public GameObject dog;                              // Sleep hier de hond in inspector
    private bool diggingStarted = false;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Wordt aangeroepen door konijn wanneer het gesettled is
    /// </summary>
    public void RabbitSettled(RabbitRunAwayController rabbit)
    {
        if (settledRabbits.Contains(rabbit))
            return;

        settledRabbits.Add(rabbit);
        Debug.Log($"Konijn gesettled! Totaal: {settledRabbits.Count}/{totalRabbits}");

        // Hond activeren bij het 3e konijn
        if (settledRabbits.Count == 3 && dog != null)
        {
            dog.SetActive(true);
            Debug.Log("Hond wordt geactiveerd bij 3e konijn!");
        }

        // Start burrow/graven timer pas als alle konijnen gesettled zijn
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
    }
}
