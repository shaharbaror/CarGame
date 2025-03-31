using UnityEngine;

public class CarRaycaster : MonoBehaviour
{
    public int rayCount = 5;
    public float rayLength = 10f;
    public float rayAngleSpread = 90f;
    public CarControl carControl; // Your car control script
    private int additionalInputs = 1; // Speed and rotation to wall

    private float[] rayDistances;
    private RaycastHit[] raycastHits;

    void Start()
    {
        rayDistances = new float[rayCount];
        raycastHits = new RaycastHit[rayCount];
    }

    public float[] GetRaycastObservations()
    {
        float angleStep = rayAngleSpread / (rayCount - 1);
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -rayAngleSpread / 2 + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
            int hits = Physics.RaycastNonAlloc(transform.position, rayDirection, raycastHits, rayLength);
            if (hits > 0)
            {
                Debug.DrawRay(transform.position, rayDirection * raycastHits[0].distance, Color.red); // Draw red if hit
                rayDistances[i] = raycastHits[i].distance / rayLength; // Normalized distance
            }
            else
            {
                Debug.DrawRay(transform.position, rayDirection * rayLength, Color.green); // Draw green if no hit
                rayDistances[i] = 1f; // Max distance if no hit
            }
        }
        return rayDistances;
    }

    public float[] GetNetworkInput()
    {
        float speed = carControl.carSpeed / carControl.maxSpeed;
        float[] raycasts = GetRaycastObservations();
        float[] networkInput = new float[raycasts.Length + additionalInputs];
        networkInput[0] = speed;
        for (int i = 0; i < raycasts.Length; i++)
        {
            networkInput[i + additionalInputs] = raycasts[i];
        }
        return networkInput;
    }

    public int GetInputSize()
    {
        return rayCount + additionalInputs;
    }

    
}