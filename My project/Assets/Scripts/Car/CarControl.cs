using UnityEngine;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 2000;
    public float brakeTorque = 2000;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public float centreOfGravityOffset = -1f;

    public float carSpeed = 0;
    public float curSteer = 0;

    public WheelControl[] wheels;
    Rigidbody rigidBody;

    public Vector3 startingPos;
    private Quaternion startingRot;

    public bool isAI = true;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        // Adjust center of mass vertically, to help prevent the car from rolling
        rigidBody.centerOfMass += Vector3.up * centreOfGravityOffset;

        // Find all child GameObjects that have the WheelControl script attached
        wheels = GetComponentsInChildren<WheelControl>();

        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAI)
        {
            float vInput = Input.GetAxis("Vertical");
            float hInput = Input.GetAxis("Horizontal");

            // Calculate current speed in relation to the forward direction of the car
            // (this returns a negative number when traveling backwards)
            float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
            this.carSpeed = forwardSpeed;


            // Calculate how close the car is to top speed
            // as a number from zero to one
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

            // Use that to calculate how much torque is available 
            // (zero torque at top speed)
            float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

            // �and to calculate how much to steer 
            // (the car steers more gently at top speed)
            float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

            // Check whether the user input is in the same direction 
            // as the car's velocity
            bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

            foreach (var wheel in wheels)
            {
                // Apply steering to Wheel colliders that have "Steerable" enabled
                if (wheel.steerable)
                {
                    wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
                }

                if (isAccelerating)
                {
                    // Apply torque to Wheel colliders that have "Motorized" enabled
                    if (wheel.motorized)
                    {
                        wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                    }
                    wheel.WheelCollider.brakeTorque = 0;
                }
                else
                {
                    // If the user is trying to go in the opposite direction
                    // apply brakes to all wheels
                    wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
                    wheel.WheelCollider.motorTorque = 0;
                }

            }
        }
    }


    // a function to operate the car gas
    public void Accelerate(float amount)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        this.carSpeed = forwardSpeed;


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        foreach (var wheel in wheels)
        {
            if (wheel.motorized)
            {
                
                    wheel.WheelCollider.motorTorque = currentMotorTorque * amount;
               
                
            }
            wheel.WheelCollider.brakeTorque = 0;
        }

    }

    // a function for operating the brakes of the car
    public void Brake(bool isHard)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        this.carSpeed = forwardSpeed;


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        // Use that to calculate how much torque is available 
        // (zero torque at top speed)
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);

        foreach (var wheel in wheels)
        {
            if (isHard)
                wheel.WheelCollider.brakeTorque = brakeTorque;
            else 
                wheel.WheelCollider.brakeTorque = brakeTorque / 2;

            wheel.WheelCollider.motorTorque = 0;
        }
    }

    // a function to handle turning wheels
    public void TurnWheel(float amount)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);


        // Calculate how close the car is to top speed
        // as a number from zero to one
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = amount * currentSteerRange;
                curSteer = amount * currentSteerRange;
            }
        }
    }

    public void ResetCar()
    {
        // Reset the car's position rotation and acceleration
        
        foreach (var wheel in wheels)
        {
            if (wheel.motorized)
            {
                wheel.WheelCollider.motorTorque = 0;
            }
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = 0;
            }
            wheel.WheelCollider.brakeTorque = 0;
        }
        rigidBody.linearVelocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        this.transform.position = startingPos;
        this.transform.rotation = startingRot;

        
    }

}