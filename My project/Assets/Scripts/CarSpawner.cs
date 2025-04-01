using NUnit.Framework;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject carPrefab;

    private GameObject[] cars;
    private CarAgent[] carAgents;
    public int carCount = 1;

    //private SpecialQueue WheelMemory, MotorMemory;
    private DQN WheelDqn, MotorDqn;

    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10 }; // Hidden layers and output layer
    public float learningRate = 0.08f;
    public float discountFactor = 0.9f;
    public int networkSyncRate = 15;
    public int replayMemorySize = 1000;
    public int batchSize = 128;

    private List<float[,]> FirstWeights;

    private float tim = 0.5f;

    public int EpisodeCount = 0;


    void Start()
    {
        if (carPrefab == null)
        {
            Debug.Log("Car prefab is null");
        }
        cars = new GameObject[carCount];
        carAgents = new CarAgent[carCount];

        _inputLayer = carPrefab.GetComponent<CarRaycaster>().GetInputSize();

        WheelDqn = new DQN(_inputLayer, _layers, 5, new Dictionary<string, (int, float)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)}
        });

        // The DQN network for the Motors
        MotorDqn = new DQN(_inputLayer, _layers, 5, new Dictionary<string, (int, float)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)}
        });

        // set up the experiance replay buffer
        //WheelMemory = new SpecialQueue(replayMemorySize);
        //MotorMemory = new SpecialQueue(replayMemorySize);
        FirstWeights = new List<float[,]>();
        for (int i = 0; i < _layers.Length; i++)
        {
            FirstWeights.Add(WheelDqn.Policy.GetLayerIndexWeights(i));
        }


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
        if (allCarsStopped && tim < Time.time)
        {
            // means end of episode
            // Teach the WheelDQN
            // Tech the MotorDQN
            TeachDQN();
            for (int i = 0; i < carCount; i++)
            {
                EpisodeCount++;
                carAgents[i].ResetCar(carAgents[i].MotorMemory.Length() == 0);
                carAgents[i].GetData((DQN)WheelDqn, MotorDqn);
            }
            if (EpisodeCount % networkSyncRate == 0)
            {
                WheelDqn.UpdateTarget();
                MotorDqn.UpdateTarget();
            }
            tim = Time.time + 0.5f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool isSame = true;
            for (int i = 0; i < FirstWeights.Count; i++)
            {
                float[,] curW = WheelDqn.Policy.GetLayerIndexWeights(i);
                for (int j = 0; j < curW.GetLength(0); j++)
                {
                    for (int k = 0; k < curW.GetLength(1); k++)
                    {
                        if (curW[j, k] != FirstWeights[i][j, k])
                        {
                            isSame = false;
                            break;
                        }
                    }
                }
            }

            Debug.Log(isSame);
        }
    }

    

    private void TeachDQN()
    {
        
        foreach (CarAgent c in carAgents)
        {
            NeuralState[] m, w;
            if (c.MotorMemory.Length() < batchSize || c.WheelMemory.Length() < batchSize)
            {
                continue;
            }
            (m, w) = c.GiveMemoriesBatch();
            //MDqn.TeachTheNetwork(m);
            WheelDqn.TeachTheNetwork(w);
        }
    }


    public void SpawnAllCars()
    {
        for (int i = 0; i < carCount; i++)
        {
            cars[i] = Instantiate(carPrefab, new Vector3(115.3f, 0.2f, 14.46f), Quaternion.Euler(0,90,0));
            carAgents[i] = cars[i].GetComponent<CarAgent>();
            carAgents[i].GetData(WheelDqn, MotorDqn);
        }
        
    }
}
