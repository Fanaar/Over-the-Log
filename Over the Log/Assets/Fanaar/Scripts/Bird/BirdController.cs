using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float circleRadius = 3f;
    public float circleSpeed = 2f;
    public float flySpeed = 5f;
    public Transform[] waypoints;

    private int currentWaypoint = -1;
    private bool isCircling = true;
    private bool isFlying = false;
    private Vector3 circleCenter;
    private float angle;

    void Start()
    {
        circleCenter = transform.position; // startpositie van de vogel
    }

    void Update()
    {
        if (isCircling)
            CircleAround();

        if (isFlying)
            FlyToWaypoint();
    }

    void CircleAround()
    {
        angle += Time.deltaTime * circleSpeed;
        float x = Mathf.Cos(angle) * circleRadius;
        float z = Mathf.Sin(angle) * circleRadius;
        transform.position = new Vector3(circleCenter.x + x, circleCenter.y, circleCenter.z + z);
        transform.LookAt(circleCenter); // Vogel kijkt naar midden
    }

    void FlyToWaypoint()
    {
        if (currentWaypoint >= waypoints.Length) return;

        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, flySpeed * Time.deltaTime);

        // Alleen de visuele vogel (child) draait naar de richting
        if (transform.childCount > 0)
        {
            Transform birdModel = transform.GetChild(0);
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // horizontaal houden
            if (direction != Vector3.zero)
                birdModel.forward = Vector3.Lerp(birdModel.forward, direction, Time.deltaTime * 5f);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            isFlying = false;
            if (currentWaypoint == waypoints.Length - 1)
            {
                FlyForward();
            }
        }
    }


    public void GoToNextWaypoint()
    {
        // Stop cirkelen bij eerste trigger
        if (isCircling)
            isCircling = false;

        if (currentWaypoint < waypoints.Length - 1)
        {
            currentWaypoint++;
            isFlying = true;
        }
    }

    void FlyForward()
    {
        // Laatste fase: recht vooruit blijven vliegen
        isFlying = false;
        StartCoroutine(FlyStraightForward());
    }

    IEnumerator FlyStraightForward()
    {
        while (true)
        {
            transform.position += transform.forward * flySpeed * Time.deltaTime;
            yield return null;
        }
    }
}
