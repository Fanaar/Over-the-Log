using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [HideInInspector] public bool isGrounded;

    void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
            isGrounded = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
            isGrounded = false;
    }
}
