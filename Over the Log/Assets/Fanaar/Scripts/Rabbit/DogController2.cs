using UnityEngine;

public class DogController2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;          // speler die hij volgt
    public float speed = 5f;          // bewegingssnelheid van de hond
    public float rotationSpeed = 5f;  // hoe snel hij draait naar de speler

    [HideInInspector]
    public bool isInPlayerTrigger = false; // stil in trigger

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return; // hond nog niet geactiveerd

        if (isInPlayerTrigger)
            return; // stil blijven in trigger

        if (player == null)
            return;

        // Beweeg naar speler
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // alleen horizontaal

        if (direction.sqrMagnitude > 0.01f)
        {
            // Rotatie naar speler
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Beweging
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}
