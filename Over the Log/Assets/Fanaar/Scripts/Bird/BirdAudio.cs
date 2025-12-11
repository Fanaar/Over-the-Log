using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BirdAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip birdCall;       // Sleep hier je geluid in
    public float minDelay = 14f;     // minimale tijd tussen kreten
    public float maxDelay = 16f;     // maximale tijd tussen kreten
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f); // kleine variatie in toonhoogte

    private AudioSource audioSource;
    private float nextCallTime;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ScheduleNextCall();
    }

    void Update()
    {
        if (Time.time >= nextCallTime)
        {
            PlayBirdCall();
            ScheduleNextCall();
        }
    }

    void PlayBirdCall()
    {
        if (birdCall == null || audioSource == null) return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(birdCall);
    }

    void ScheduleNextCall()
    {
        float delay = Random.Range(minDelay, maxDelay);
        nextCallTime = Time.time + delay;
    }
}
