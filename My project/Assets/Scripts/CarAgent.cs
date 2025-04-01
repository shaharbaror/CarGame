using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using Unity.MLAgents.Actuators;
using UnityEngine.UIElements;

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
    public int networkSyncRate = 100;
    public int replayMemorySize = 1000;
    public int batchSize = 128;

    public float cooldown = 100f;
    private float lastTime = 0;

    public float SPEEEDUWAGOOON = 10f;

    int wheelAction;
    int motorAction;

    public bool sessionPlaying = true;
    public int _actionCount = 0;
    private int _maxActions = 50;


    private float _motorReward = 0f;
    public float _wheelReward = 0f;
    private int _motorRewardCount = 0;
    private int _wheelRewardCount = 0;
    private float[] _lastState;
    private float[] _currentState;

    private bool once = true;



    public void Awake()
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
            {"batchSize", (batchSize, 0)}
        });

        // The DQN network for the Motors
        MotorDqn = new DQN(_inputLayer, _layers, 5, new System.Collections.Generic.Dictionary<string, (int, float)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)}
        });

        WheelMemory = new SpecialQueue(replayMemorySize);
        MotorMemory = new SpecialQueue(replayMemorySize);

        
        
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
            case 4:
                // turn easy left
                carControl.TurnWheel(0.5f);
                break;
            case 5:
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
        //switch (action)
        //{
        //    case 0:
                
        //        // Hard Acceleration
        //        carControl.Accelerate(true);
        //        break;
        //    case 1:
                
        //        // Soft Acceleration
        //        carControl.Accelerate(false);
        //        break;
        //    case 2:
        //        // Soft Brake
        //        carControl.Brake(false);
        //        break;
        //    case 3:
        //        // Hard brake
        //        carControl.Brake(true);
        //        break;
        //    default:
                
        //        // Do nothing, keep the same speed
        //        break;
        //}
        carControl.Accelerate(false);
    }

    public void Update()
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
                motorAction = MotorDqn.GetAction(state);
                // Apply the action to the car
                PreformWheelAction(wheelAction);
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

        } else if (once)
        {
            CalculateWheelReward();
            CalculateMotorReward();
            _currentState = carRaycaster.GetNetworkInput();
            MotorMemory.PushQueue(new NeuralState(_lastState, motorAction, _motorReward, _currentState, true));
            WheelMemory.PushQueue(new NeuralState(_lastState, wheelAction, _wheelReward, _currentState, true));
            once = false;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale += 1;

        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            Time.timeScale -= 1;
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
         if (carControl.carSpeed >0)
        _motorReward += carControl.carSpeed * 0.01f;
        else
        {
            _motorReward -= 0.1f;
        }
    }

    private void CalculateWheelReward()
    {
        // Calculate the reward based on the cars distance from the center of the track
        Quaternion rotation = transform.rotation;
        Vector3 right = rotation * Vector3.right;
        Vector3 left = rotation * Vector3.left;
        RaycastHit hRight, hLeft;
        float distance = 20f;
        Physics.Raycast(transform.position, right, out hRight, distance);
        Debug.DrawRay(transform.position, right * hRight.distance, Color.red);
        Physics.Raycast(transform.position, left, out hLeft, distance);
        Debug.DrawRay(transform.position, left * hLeft.distance, Color.red);
        if (carControl.carSpeed > 0)
        {
            _wheelReward += Mathf.Min(hRight.distance, hLeft.distance) / Mathf.Max(hRight.distance, hLeft.distance) * 3; // adds between 0 to 1 depends on the distance from the center
            
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("BadWall"))
        {
            //_motorReward -= 10f;
            _wheelReward -= 10f;
            sessionPlaying = false;
        }
        if (collision.gameObject.CompareTag("Good Wall"))
        {
            //_motorReward += 10f;
            _wheelReward += 10f;
            sessionPlaying = false;
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