using UnityEngine;

public class CarRaycaster : MonoBehaviour
{
    public int rayCount = 5;
    public float rayLength = 10f;
    public float rayAngleSpread = 90f;
    public float rayHight = 0f;
    public bool isAuto = false;
    public CarControl carControl; // Your car control script
    private int additionalInputs = 4; // Speed rotation aceleration and distance to wall

    private float[] rayDistances;
    private RaycastHit[] raycastHits;

    void Start()
    {
        rayDistances = new float[rayCount];
        raycastHits = new RaycastHit[rayCount];
        if (isAuto)
        {
            rayHight = transform.position.y;
        }
    }

    //public float[] GetRaycastObservations()
    //{
    //    float angleStep = rayAngleSpread / (rayCount - 1);
    //    for (int i = 0; i < rayCount; i++)
    //    {
    //        float angle = -rayAngleSpread / 2 + i * angleStep;
    //        Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
    //        int hits = Physics.RaycastNonAlloc(transform.position, rayDirection, raycastHits, rayLength);
    //        if (hits > 0)
    //        {
    //            Debug.DrawRay(transform.position, rayDirection * raycastHits[0].distance, Color.red); // Draw red if hit
    //            rayDistances[i] = raycastHits[i].distance / rayLength; // Normalized distance
    //        }
    //        else
    //        {
    //            Debug.DrawRay(transform.position, rayDirection * rayLength, Color.green); // Draw green if no hit
    //            rayDistances[i] = 1f; // Max distance if no hit
    //        }
    //    }
    //    return rayDistances;
    //}
    

public float[] GetRaycastObservationsBadWall()
{
    float angleStep = rayAngleSpread / (rayCount - 1);
    Vector3 rayPos = new Vector3(transform.position.x, rayHight, transform.position.z);
    for (int i = 0; i < rayCount; i++)
    {
        float angle = -rayAngleSpread / 2 + i * angleStep;
        Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;

        int layerMask = LayerMask.GetMask("Bad Layer"); // Get the layer mask for "BadLayer"

        int hits = Physics.RaycastNonAlloc(rayPos, rayDirection, raycastHits, rayLength, layerMask);

        if (hits > 0)
        {
            //Check if the hit object has the correct tag. This is redundant if your layer mask is set up correctly, but a good safety measure.
            if (raycastHits[0].collider.gameObject.CompareTag("BadWall"))
            {
                Debug.DrawRay(rayPos, rayDirection * raycastHits[0].distance, Color.red); // Draw red if hit
                rayDistances[i] = raycastHits[0].distance / rayLength; // Normalized distance
            }
            else
            {
                Debug.DrawRay(rayPos, rayDirection * rayLength, Color.green); // Draw green if no hit
                rayDistances[i] = 1f; // Max distance if no hit.
            }

        }
        else
        {
            Debug.DrawRay(rayPos, rayDirection * rayLength, Color.green); // Draw green if no hit
            rayDistances[i] = 1f; // Max distance if no hit
        }
    }
    return rayDistances;
}

    public float[] GetSides()
    {
        float angleStep = 180f;
        Vector3 rayPos = new Vector3(transform.position.x, rayHight, transform.position.z);
        for (int i = 0; i < 2; i++)
        {
            float angle = -90 + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;

            int layerMask = LayerMask.GetMask("Bad Layer"); // Get the layer mask for "BadLayer"

            int hits = Physics.RaycastNonAlloc(rayPos, rayDirection, raycastHits, rayLength, layerMask);

            if (hits > 0)
            {
                //Check if the hit object has the correct tag. This is redundant if your layer mask is set up correctly, but a good safety measure.
                if (raycastHits[0].collider.gameObject.CompareTag("BadWall"))
                {
                    //Debug.DrawRay(rayPos, rayDirection * raycastHits[0].distance, Color.white); // Draw red if hit
                    rayDistances[i] = raycastHits[0].distance / rayLength; // Normalized distance
                }
                else
                {
                   // Debug.DrawRay(rayPos, rayDirection * rayLength, Color.yellow); // Draw green if no hit
                    rayDistances[i] = 1f; // Max distance if no hit.
                }

            }
            else
            {
                //Debug.DrawRay(rayPos, rayDirection * rayLength, Color.yellow); // Draw green if no hit
                rayDistances[i] = 1f; // Max distance if no hit
            }
        }
        return rayDistances;    // [0] is left, [1] is right
    }

    public float[] GetRaycastObservations()
    {
        float angleStep = rayAngleSpread / (rayCount - 1);
        Vector3 rayPos = new Vector3(transform.position.x, rayHight, transform.position.z);
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -rayAngleSpread / 2 + i * angleStep;
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;


            RaycastHit hit;
            Physics.Raycast(rayPos, rayDirection,out hit, rayLength);

            if (hit.distance != rayLength)
            {
                //Check if the hit object has the correct tag. This is redundant if your layer mask is set up correctly, but a good safety measure.
               
                
                Debug.DrawRay(rayPos, rayDirection * hit.distance, Color.red); // Draw red if hit
                rayDistances[i] = hit.distance / rayLength; // Normalized distance

            }
            else
            {
                Debug.DrawRay(rayPos, rayDirection * rayLength, Color.green); // Draw green if no hit
                rayDistances[i] = 1f; // Max distance if no hit
            }
        }
        return rayDistances;
    }

    public float[] GetNetworkInput()
    {
        float speed = carControl.carSpeed / carControl.maxSpeed;
        float rotations = carControl.curSteer / carControl.steeringRange;
        float acceleration = (carControl.carSpeed - carControl.prevSpeed) / Time.deltaTime / 5f; // Normalized acceleration

        float[] sides = GetSides();
        float centeringMetric = Mathf.Min(sides[0], sides[1]) / Mathf.Max(sides[0], sides[1]);

        float[] raycasts = GetRaycastObservationsBadWall();
        
        float[] networkInput = new float[raycasts.Length + additionalInputs];
        networkInput[0] = speed;
        networkInput[1] = rotations;
        networkInput[2] = acceleration;
        networkInput[3] = centeringMetric;

        for (int i = 0; i < raycasts.Length; i++)
        {
            networkInput[i + additionalInputs] = raycasts[i];
        }
        return networkInput;
    }

    public int GetInputSize()
    {
        //Debug.Log($"giving them {rayCount + additionalInputs} when add is {additionalInputs} and count is {rayCount}");
        return rayCount + additionalInputs;
    }

    
}