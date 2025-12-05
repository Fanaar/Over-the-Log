using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class DiveHandler : MonoBehaviour
{
    public KeyCode diveKey = KeyCode.Mouse0;
    public float diveSpeed = 30f;
    public float diveRotationSpeed = 5f;
    public float diveStopDistance = 0.5f;
    public AudioClip diveSound;

    public PreyDetector catchTrigger; // inner trigger
    public PreyDetector missTrigger;  // outer trigger

    private CharacterController controller;
    private bool isDiving = false;
    private bool canDive = true;

    void Awake() => controller = GetComponent<CharacterController>();

    public void HandleDive(PlayerInputHandler input)
    {
        if (input.DivePressed && canDive && !isDiving)
        {
            TryCatchPrey();
        }
    }

    void TryCatchPrey()
    {
        if (!canDive || isDiving) return;

        canDive = false;
        StartCoroutine(DiveCooldown());

        if (catchTrigger.preyInRange && catchTrigger.currentPrey != null)
        {
            StartCoroutine(DiveToPrey(catchTrigger.currentPrey.transform));
        }
        else if (missTrigger.preyInRange && missTrigger.currentPrey != null)
        {
            StartCoroutine(DiveAndMiss(missTrigger.currentPrey.transform));
        }
        else
        {
            Debug.Log("Too far to dive!");
        }
    }

    IEnumerator DiveCooldown()
    {
        yield return new WaitForSeconds(1.5f);
        canDive = true;
    }

    IEnumerator DiveToPrey(Transform prey)
    {
        isDiving = true;
        if (diveSound != null) AudioSource.PlayClipAtPoint(diveSound, transform.position);

        while (Vector3.Distance(transform.position, prey.position) > diveStopDistance)
        {
            Vector3 dir = (prey.position - transform.position).normalized;
            controller.Move(dir * diveSpeed * Time.deltaTime);

            // Rotate towards prey
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * diveRotationSpeed);
            yield return null;
        }

        isDiving = false;
        Debug.Log("Caught prey!");
    }

    IEnumerator DiveAndMiss(Transform prey)
    {
        isDiving = true;
        if (diveSound != null) AudioSource.PlayClipAtPoint(diveSound, transform.position);

        Vector3 missTarget = prey.position + transform.forward * 3f; // just an example
        while (Vector3.Distance(transform.position, missTarget) > 0.5f)
        {
            Vector3 dir = (missTarget - transform.position).normalized;
            controller.Move(dir * diveSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * diveRotationSpeed);
            yield return null;
        }

        isDiving = false;
        Debug.Log("Missed prey!");
    }
}
