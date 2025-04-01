using NUnit.Framework;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject carPrefab;

    private GameObject[] cars;
    private CarAgent[] carAgents;
    public int carCount = 1;

    private SpecialQueue WheelMemory, MotorMemory;
    private DQN WheelDqn, MotorDqn;

    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10 }; // Hidden layers and output layer
    public float learningRate = 0.08f;
    public float discountFactor = 0.9f;
    public int networkSyncRate = 100;
    public int replayMemorySize = 1000;
    public int batchSize = 32;


    void Start()
    {
        if (carPrefab == null)
        {
            Debug.Log("Car prefab is null");
        }
        cars = new GameObject[carCount];
        carAgents = new CarAgent[carCount];

        _inputLayer = carPrefab.GetComponent<CarRaycaster>().GetInputSize();

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

        // set up the experiance replay buffer
        WheelMemory = new SpecialQueue(replayMemorySize);
        MotorMemory = new SpecialQueue(replayMemorySize);

        SpawnAllCars();
    }

    // Update is called once per frame
    void Update()
    {
        bool allCarsStopped = true;
        for ( int i = 0; i < carCount && allCarsStopped; i++)
        {
            if (carAgents[i].sessionPlaying)
            {
                allCarsStopped = false;
            }
        }
        if (allCarsStopped)
        {
            // means end of episode
            // Teach the WheelDQN
            // Tech the MotorDQN
            TeachDQN(WheelDqn, WheelMemory);
            TeachDQN(MotorDqn, MotorMemory);
            for (int i = 0; i < carCount; i++)
            {
                carAgents[i].ResetCar();
            }
        }
    }

    private void TeachDQN(DQN dqn, SpecialQueue qe)
    {
        NeuralState[] batch = qe.ClearAtRandom(batchSize);
        dqn.TeachTheNetwork(batch);
    }


    public void SpawnAllCars()
    {
        for (int i = 0; i < carCount; i++)
        {
            cars[i] = Instantiate(carPrefab, new Vector3(115.3f, 0.5f, 14.46f), Quaternion.Euler(0,90,0));
            carAgents[i] = cars[i].GetComponent<CarAgent>();
            carAgents[i].GetData(WheelDqn, MotorDqn, WheelMemory, MotorMemory);
        }
        
    }
}
