using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using Unity.MLAgents.Actuators;

public class CarAgent : MonoBehaviour
{
    private NeuralNet neuralNet;
    private RayPerceptionSensorComponent3D raySensor;


    // Neural Network Configuration
    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10, 2 }; // Hidden layers and output layer
    public float learningRate = 0.01f;

    
}