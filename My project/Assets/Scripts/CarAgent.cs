using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using Unity.MLAgents.Actuators;
public class CarAgent : MonoBehaviour
{
    private DQN WheelDqn, MotorDqn;

    
    public CarControl carControl;
    public CarRaycaster carRaycaster;


    // Neural Network Configuration
    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10 }; // Hidden layers and output layer
    public float learningRate = 0.08f;
    public float discountFactor = 0.9f;
    public int networkSyncRate = 100;
    public int replayMemorySize = 1000;
    public int batchSize = 32;

    public float cooldown = 100f;
    private float lastTime = 0;

    int wheelAction;
    int motorAction;



    public void Start()
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

        // Create the DQN
        // The DQN network for the Wheels
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
        switch (action)
        {
            case 0:
                Debug.Log("Hard Acceleration");
                // Hard Acceleration
                carControl.Accelerate(true);
                break;
            case 1:
                Debug.Log("Soft Acceleration");
                // Soft Acceleration
                carControl.Accelerate(false);
                break;
            case 2:
                // Soft Brake
                carControl.Brake(false);
                break;
            case 3:
                // Hard brake
                carControl.Brake(true);
                break;
            default:
                Debug.Log(action);
                // Do nothing, keep the same speed
                break;
        }
    }

    public void Update()
    {
        if (Time.time > lastTime)
        {
            lastTime = Time.time + cooldown;


            // Get the state from the ray sensor
            float[] state = carRaycaster.GetNetworkInput();
            // Get the action form the DQN
            wheelAction = WheelDqn.GetAction(state);
            motorAction = MotorDqn.GetAction(state);
            // Apply the action to the car
            PreformWheelAction(wheelAction);
            PreformMotorAction(motorAction);
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