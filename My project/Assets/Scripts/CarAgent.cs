using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using Unity.MLAgents.Actuators;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Net;
using Assets.Scripts.FileHandler;
using Unity.VisualScripting;

public class CarAgent : MonoBehaviour
{
    private DQN WheelDqn, MotorDqn;
    public SpecialQueue WheelMemory, MotorMemory;


    public CarControl carControl;
    public CarRaycaster carRaycaster;


    // Neural Network Configuration
    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10 }; // Hidden layers and output layer
    public float learningRate = 0.08f;
    public float discountFactor = 0.9f;
    public float epsilon = 0.5f;
    public int networkSyncRate = 100;
    public int replayMemorySize = 10000;
    public int batchSize = 128;
    

    public float cooldown = 100f;
    private float lastTime = 0;

    public float SPEEEDUWAGOOON = 10f;

    int wheelAction;
    int motorAction;

    public bool sessionPlaying = true;
    public int _actionCount = 0;
    private int _maxActions = 150;


    private float _motorReward = 0f;
    public float _wheelReward = 0f;
    private int _motorRewardCount = 0;
    private int _wheelRewardCount = 0;
    private float[] _lastState;
    private float[] _currentState;

    private bool once = true;
    public bool toggle = true;



    public void Awake()
    {
        if (toggle)
        {
            // Create the Ray Perception Sensor
            carControl = GetComponent<CarControl>();
            if (carControl == null)
            {
                Debug.Log("No Connection with the Car Controller");
            }

            carRaycaster = GetComponent<CarRaycaster>();
            if (carRaycaster == null)
            {
                Debug.Log("No Connection with the Car Raycaster");
            }

            _inputLayer = carRaycaster.GetInputSize();

            //// Create the DQN
            //// The DQN network for the Wheels
            WheelDqn = new DQN(_inputLayer, _layers, 5, new System.Collections.Generic.Dictionary<string, (int, float)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)},
            {"epsilon",(0, epsilon) },
        });

            // The DQN network for the Motors
            MotorDqn = new DQN(_inputLayer, _layers, 5, new System.Collections.Generic.Dictionary<string, (int, float)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)},
            {"epsilon",(0, epsilon) },
        });


            WheelMemory = new SpecialQueue(replayMemorySize);
            MotorMemory = new SpecialQueue(replayMemorySize);

        }
        
        }

    public void PreformWheelAction(int action)
    {
        // Apply the action to the car
        switch(action)
        {
            case 0:
                // Turn hard left
                carControl.TurnWheel(-1f);
                break;
            case 1:
                // Turn easy left
                carControl.TurnWheel(-0.5f);
                break;
            case 2:
                // Move forward
                carControl.TurnWheel(0);
                break;
            case 3:
                // turn easy left
                carControl.TurnWheel(0.5f);
                break;
            case 4:
                // turn hard left
                carControl.TurnWheel(1f);
                break;
            default:
                // Do nothing
                break;
        }
    }

    public void PreformMotorAction(int action)
    {
        switch (action)
        {
            case 0:

                // Hard Acceleration
                carControl.Accelerate(1);
                break;
            case 1:

                // Soft Acceleration
                carControl.Accelerate(0.5f);
                break;
            case 2:
                // Soft Brake
                carControl.Accelerate(-0.5f);
                break;
            case 3:
                // Hard brake
                carControl.Accelerate(-1f);
                break;
            default:
                carControl.Accelerate(0);
                // Do nothing, keep the same speed
                break;
        }
        //carControl.Accelerate(false);
    }

    public void Update()
    {
        if (toggle)
        {
            if (sessionPlaying == true)
            {
                if (Time.time > lastTime)
                {
                    lastTime = Time.time + cooldown;

                    CalculateMotorReward();
                    CalculateWheelReward();
                    if (_motorReward > 0)
                    {
                        _motorRewardCount++;
                    }
                    if (_wheelReward > 0)
                    {
                        _wheelRewardCount++;
                    }
                    // Get the state from the ray sensor
                    float[] state = carRaycaster.GetNetworkInput();
                    _currentState = state;

                    MotorMemory.PushQueue(new NeuralState(_lastState, motorAction, _motorReward, _currentState, false));
                    WheelMemory.PushQueue(new NeuralState(_lastState, wheelAction, _wheelReward, _currentState, false));

                    // Get the action form the DQN
                    wheelAction = WheelDqn.GetAction(state);
                    PreformWheelAction(wheelAction);

                    //// THIS PART IS VERY IMPORTANT BECAUSE IT ONLY WORKS IF THE ROTATION IS ON THE SECOND ELEMENT!!!!!!
                    //state[1] = carControl.curSteer / carControl.steeringRange;
                    ////HERERERERERERERE
                    //// WE WANT THE MOTOR TO PREFORM BASED ON THE STEERING WHEEL DESCISION

                    motorAction = MotorDqn.GetAction(state);
                    // Apply the action to the car
                   
                    PreformMotorAction(motorAction);

                    _actionCount++;
                }
                if (_actionCount >= _maxActions)
                {
                    sessionPlaying = false;
                }
                _motorReward = 0f;
                _wheelReward = 0f;
                _lastState = _currentState;

            }
            else if (once)
            {
                CalculateWheelReward();
                CalculateMotorReward();
                _currentState = carRaycaster.GetNetworkInput();
                MotorMemory.PushQueue(new NeuralState(_lastState, motorAction, _motorReward, _currentState, false));
                WheelMemory.PushQueue(new NeuralState(_lastState, wheelAction, _wheelReward, _currentState, false));
                once = false;
            }

            
        }
        //else
        //{
        //    PreformWheelAction(wheelAction);
        //    PreformMotorAction(motorAction);

        //}

        // Set a reward based on the action

        // Get the termination status

        // Add the experience to the memory

        // Get the new state from the ray sensor

        // Train the network



    }

    public void GetData(DQN wheels, DQN motors)
    {
        WheelDqn.Clone(wheels);
        MotorDqn.Clone(motors);
    }


    private void CalculateMotorReward()
    {
         if (carControl.carSpeed >0.5f)
            _motorReward += carControl.carSpeed * 0.3f;
        else
        {
            _motorReward -= 0.4f;
        }
    }

    private void CalculateWheelReward()
    {
        // Calculate the reward based on the cars distance from the center of the track
        //Quaternion rotation = transform.rotation;
        //Vector3 left = Quaternion.Euler(0, -90, 0) * transform.forward;
        //Vector3 right = Quaternion.Euler(0, 90, 0) * transform.forward;
        //RaycastHit hRight, hLeft;
        //float distance = 20f;

        //Vector3 rayPos = new Vector3(transform.position.x, carRaycaster.rayHight, transform.position.y);
        //// NOT GOOD RAYCASTING
        //Physics.Raycast(rayPos, right, out hRight, distance);
        //Debug.DrawRay(rayPos, right * hRight.distance, Color.blue);
        //Physics.Raycast(rayPos, left, out hLeft, distance);
        //Debug.DrawRay(rayPos, left * hLeft.distance, Color.white);
        float[] distances = carRaycaster.GetSides();
        if (carControl.carSpeed > 0)
        {
            float incAmount = Mathf.Pow(Mathf.Min(distances[0],distances[1]) / Mathf.Max(distances[0], distances[1]), 2); // adds between 0 to 1 depends on the distance from the center
            
           
            if (incAmount > 0.5)
            {
                _wheelReward += incAmount;
            }
            else
            {
                _wheelReward -= 0.3f;
            }

            //float incAmount = Mathf.Pow(hRight.distance / 4f, 2);
            //if (incAmount > 1.3)
            //{
            //    _wheelReward -= -0.3f;
            //}
            //else if (incAmount > 1)
            //{
            //    _wheelReward += 1.3f - incAmount;
            //}
            //else if (incAmount >= 0.3)
            //{
            //    _wheelReward += incAmount;
            //}
            //else
            //{
            //    _wheelReward -= 0.3f;
            //}

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("BadWall"))
        {
            _motorReward -= 1f;
            _wheelReward -= 10f;
            sessionPlaying = false;
        }
        if (collision.gameObject.CompareTag("Good Wall"))
        {
            _motorReward += 10f;
            //_wheelReward += 10f;
           
        }
        if (collision.gameObject.CompareTag("Respawn"))
        {
            sessionPlaying = false;
            Debug.Log("FINISHED!");
        }

    }

    

    public void ResetCar(bool isMemory)
    {
        carControl.ResetCar();
        sessionPlaying = true;
        once = true;
        _actionCount = 0;
        _motorRewardCount = 0;
        _wheelRewardCount = 0;
        if (isMemory)
        {
            MotorMemory.Clear();
            WheelMemory.Clear();
        }
    }

    public (NeuralState[], NeuralState[]) GiveMemoriesBatch()
    {
        return (MotorMemory.ClearAtRandom(batchSize), WheelMemory.ClearAtRandom(batchSize));
    }

    public void SetAgent(Dictionary<string, int> changes)
    {
        this.replayMemorySize = changes["replayMemorySize"];
        this.MotorMemory.ChangeMaxSize(changes["replayMemorySize"]);
        this.WheelMemory.ChangeMaxSize(changes["replayMemorySize"]);
        this.batchSize = changes["batchSize"];
        this._maxActions = changes["maxActions"];
    }

    public void ChangeMaxActions(int action) {
    this._maxActions = action;
    }





    //public override void Initialize()
    //{
    //    WheelDqn = new DQN(_inputLayer, _layers, 5, new System.Collections.Generic.Dictionary<string, (int, float)>{
    //        {"learningRate", (0, learningRate)},
    //        {"discountFactor", (0, discountFactor)},
    //        {"netwrokSyncRate", (networkSyncRate, 0)},
    //        {"replayMemorySize", (replayMemorySize, 0)},
    //        {"batchSize", (batchSize, 0)}
    //    });

    //    // The DQN network for the Motors
    //    MotorDqn = new DQN(_inputLayer, _layers, 5, new System.Collections.Generic.Dictionary<string, (int, float)>{
    //        {"learningRate", (0, learningRate)},
    //        {"discountFactor", (0, discountFactor)},
    //        {"netwrokSyncRate", (networkSyncRate, 0)},
    //        {"replayMemorySize", (replayMemorySize, 0)},
    //        {"batchSize", (batchSize, 0)}
    //    });


    //    // Create the Ray Perception Sensor
    //    carControl = GetComponent<CarControl>();
    //    if (carControl == null)
    //    {
    //        throw new System.Exception("No Connection with the Car Controller");
    //    }
    //}



}