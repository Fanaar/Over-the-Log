using UnityEngine;

public class DogController2 : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Animation")]
    public Animator animator;          // reference to the dog's Animator

    [HideInInspector]
    public bool isInPlayerTrigger = false;

    private void Update()
    {
        if (!gameObject.activeInHierarchy || player == null)
            return;

        if (isInPlayerTrigger)
        {
            // Idle if in trigger
            animator.SetBool("isWalking", false);
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0; // only horizontal movement

        if (direction.sqrMagnitude > 0.01f)
        {
            // Rotate towards player
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Move
            transform.position += transform.forward * speed * Time.deltaTime;

            // Walking animation
            animator.SetBool("isWalking", true);
        }
        else
        {
            // Idle animation
            animator.SetBool("isWalking", false);
        }
    }
}
