using UnityEngine;
using System.Collections.Generic;

public class SnakeGenerator : MonoBehaviour
{
    [Header("Snake Settings")]
    public int bodyParts = 10;
    public float spacing = 0.5f;
    public float sizeReduction = 0.1f;
    public float scaleMultiplier = 1f; // NEW: adjust overall size
    public GameObject bodyPrefab;

    [HideInInspector] public List<Transform> bodySegments = new List<Transform>();

    void Start()
    {
        GenerateSnake();
    }

    void GenerateSnake()
    {
        float currentScale = 1f;

        for (int i = 0; i < bodyParts; i++)
        {
            GameObject part = Instantiate(bodyPrefab, transform);
            part.transform.localPosition = new Vector3(0, 0, -i * spacing * scaleMultiplier); // scale spacing
            part.transform.localScale = Vector3.one * currentScale * scaleMultiplier;

            bodySegments.Add(part.transform);

            currentScale = Mathf.Max(0.2f, currentScale - sizeReduction);
        }
    }
}
