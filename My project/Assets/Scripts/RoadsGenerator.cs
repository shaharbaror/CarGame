using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public float roadRadius = 50f;
    public int roadSegments = 36;
    public GameObject roadSegmentPrefab;

    void Start()
    {
        GenerateCircularRoad();
    }

    void GenerateCircularRoad()
    {
        for (int i = 0; i < roadSegments; i++)
        {
            float angle = i * (360f / roadSegments);
            Vector3 position = Quaternion.Euler(0, angle, 0) * Vector3.forward * roadRadius;

            GameObject segment = Instantiate(roadSegmentPrefab, position, Quaternion.Euler(0, angle, 0), transform);
            segment.transform.localScale = new Vector3(10f, 0.1f, 5f);
        }
    }
}