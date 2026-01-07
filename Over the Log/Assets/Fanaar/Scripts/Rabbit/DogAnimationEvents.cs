using UnityEngine;
using System.Collections;

public class DogAnimationEvents : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnScaryFaceFinished()
    {
        StartCoroutine(ResetScaryFaceDelayed());
    }

    private IEnumerator ResetScaryFaceDelayed()
    {
        yield return new WaitForSeconds(0.5f);

        if (animator != null)
            animator.SetBool("isScaryFace", false);
    }
}
