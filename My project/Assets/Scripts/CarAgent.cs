using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using Unity.MLAgents.Actuators;

public class CarAgent : MonoBehaviour
{
    private DQN dqn;
    
    public RayPerceptionSensorComponent3D raySensor;


    // Neural Network Configuration
    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10, 2 }; // Hidden layers and output layer
    public double learningRate = 0.01;
    public double discountFactor = 0.99;
    public int networkSyncRate = 100;
    public int replayMemorySize = 1000;
    public int batchSize = 32;


    public void Start()
    {
        // Create the DQN
        dqn = new DQN(_inputLayer, _layers, 2, new System.Collections.Generic.Dictionary<string, (int, double)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)}
        });


        // Create the Ray Perception Sensor

    }

    public void PreformAction(int action)
    {
        // Apply the action to the car
        switch(action)
        {
            case 0:
                // Turn hard right
                break;
            case 1:
                // Turn easy left
                break;
            case 2:
                // Move forward
                break;
            case 3:
                // apply breaks
                break;
            case 4:
                // turn easy left
            case 5:
                // turn hard left
                break;
            default:
                // Do nothing
                break;
        }
    }

    public void Update()
    {
        // Get the state from the ray sensor
        double[] state = new double[5];
        // Get the action form the DQN
        int action = dqn.GetAction(state);
        // Apply the action to the car
        PreformAction(action);

        // Set a reward based on the action

        // Get the termination status

        // Add the experience to the memory

        // Get the new state from the ray sensor

        // Train the network



    }

}