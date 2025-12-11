using UnityEngine;

public class PreyMovement : MonoBehaviour
{
    public enum MovementType
    {
        None,
        SideToSide,
        FrontToBack,
        Circle
    }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.SideToSide;
    public float moveSpeed = 3f;
    public float moveDistance = 5f;
    public float circleRadius = 2f;

    private Vector3 startPos;
    private bool isCaught = false;
    private float timeCounter = 0f;
    private float direction = 1f;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (isCaught)
            return;

        switch (movementType)
        {
            case MovementType.SideToSide:
                MoveSideToSide();
                break;

            case MovementType.FrontToBack:
                MoveFrontToBack();
                break;

            case MovementType.Circle:
                MoveInCircle();
                break;

            case MovementType.None:
            default:
                break;
        }
    }

    // ------------------------------------
    // MOVEMENT PATTERNS
    // ------------------------------------
    private void MoveSideToSide()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + new Vector3(offset, 0, 0);
    }

    private void MoveFrontToBack()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        transform.position = startPos + new Vector3(0, 0, offset);
    }

    private void MoveInCircle()
    {
        timeCounter += Time.deltaTime * moveSpeed;

        float x = Mathf.Cos(timeCounter) * circleRadius;
        float z = Mathf.Sin(timeCounter) * circleRadius;

        transform.position = startPos + new Vector3(x, 0f, z);
    }


    // ------------------------------------
    // CAUGHT / STOPPING LOGIC
    // ------------------------------------

    public void PrepareForStop(float delay = 0.5f)
    {
        StartCoroutine(StopAfterDelay(delay));
    }

    private System.Collections.IEnumerator StopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnCaught();
    }

    public void OnCaught()
    {
        isCaught = true;
        Debug.Log("🛑 Prey stopped moving!");
    }
}
