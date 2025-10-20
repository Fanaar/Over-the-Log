using UnityEngine;

public class PlatformDescender : MonoBehaviour
{
    [SerializeField] private float descendDistance = 5f;   // how far down
    [SerializeField] private float moveSpeed = 2f;         // speed for both directions

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isDescending = false;
    private bool isAscending = false;

    private void Awake()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.down * Mathf.Abs(descendDistance);
    }

    private void Update()
    {
        if (isDescending)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) <= 0.001f)
            {
                isDescending = false; // reached bottom
            }
        }
        else if (isAscending)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPos) <= 0.001f)
            {
                isAscending = false; // back at start
            }
        }
    }

    public void StartDescending()
    {
        isAscending = false;
        isDescending = true;
    }

    public void StartAscending()
    {
        isDescending = false;
        isAscending = true;
    }
}
