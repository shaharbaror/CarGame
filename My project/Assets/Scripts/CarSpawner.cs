using NUnit.Framework;
using Unity.VisualScripting;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.FileHandler;

public class CarSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject carPrefab;

    private GameObject[] cars;
    private CarAgent[] carAgents;
    public int carCount = 1;
    public bool toggle = false; // for testing purposes

    //private SpecialQueue WheelMemory, MotorMemory;
    private DQN WheelDqn, MotorDqn;

    public int _inputLayer = 5; // Default, will be overridden
    public int[] _layers = { 10, 10 }; // Hidden layers and output layer
    public float learningRate = 0.08f;
    public float discountFactor = 0.9f;
    public float epsilon = 0.5f;
    public int networkSyncRate = 15;
    public int replayMemorySize = 1000;
    public int minAmount = 1000;
    public int batchSize = 128;

    private List<float[,]> FirstWeights;

    private float tim = 0.5f;

    public int EpisodeCount = 0;
    public int HowManyTimesTaught = 0;
    public int HowManyTimesSwitched = 0;


    void Start()
    {
        if (toggle)
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
            {"batchSize", (batchSize, 0)},
            {"epsilon",(0, epsilon) }
        });

        // The DQN network for the Motors
        MotorDqn = new DQN(_inputLayer, _layers, 5, new Dictionary<string, (int, float)>{
            {"learningRate", (0, learningRate)},
            {"discountFactor", (0, discountFactor)},
            {"netwrokSyncRate", (networkSyncRate, 0)},
            {"replayMemorySize", (replayMemorySize, 0)},
            {"batchSize", (batchSize, 0)},
            {"epsilon",(0, epsilon) }
        });

        // set up the experiance replay buffer
        //WheelMemory = new SpecialQueue(replayMemorySize);
        //MotorMemory = new SpecialQueue(replayMemorySize);
        FirstWeights = new List<float[,]>();
        for (int i = 0; i < _layers.Length; i++)
        {
            float[,] theseWs = (float[,])WheelDqn.Target.GetLayerIndexWeights(i).Clone();
            FirstWeights.Add(theseWs);
        }

        GetNets();
        SpawnAllCars();
    } else
        {

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (toggle)
        {
            bool allCarsStopped = true;
            for (int i = 0; i < carCount && allCarsStopped; i++)
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
                    carAgents[i].GetData(WheelDqn, MotorDqn);
                }
                if (EpisodeCount % networkSyncRate == 0)
                {
                    WheelDqn.UpdateTarget();
                    MotorDqn.UpdateTarget();
                    HowManyTimesSwitched++;
                }
                tim = Time.time + 0.5f;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                bool isSame = true;
                for (int i = 0; i < FirstWeights.Count; i++)
                {
                    float[,] curW = WheelDqn.Target.GetLayerIndexWeights(i);
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
            else if (Input.GetKeyDown(KeyCode.N))
            {
                this.SaveNets();
            }

            if (EpisodeCount == 3000)
            {
                epsilon = 0.3f;
                WheelDqn.ChangeEpsilon(epsilon);
                MotorDqn.ChangeEpsilon(epsilon);
            }
            if (EpisodeCount == 10000)
            {
                epsilon = 0.1f;
                WheelDqn.ChangeEpsilon(epsilon);
                MotorDqn.ChangeEpsilon(epsilon);
            }
        }
        else { }
    }

    

    private void TeachDQN()
    {
        
        foreach (CarAgent c in carAgents)
        {
            NeuralState[] m, w;
            if (c.MotorMemory.Length() < minAmount || c.WheelMemory.Length() < minAmount)
            {
                continue;
            }
            (m, w) = c.GiveMemoriesBatch();
            MotorDqn.TeachTheNetwork(m);
            WheelDqn.TeachTheNetwork(w);
            HowManyTimesTaught++;
        }
    }


    public void SpawnAllCars()
    {
        for (int i = 0; i < carCount; i++)
        {
            cars[i] = Instantiate(carPrefab, new Vector3(transform.position.x, 0.2f, transform.position.z), Quaternion.Euler(0,90,0));
            carAgents[i] = cars[i].GetComponent<CarAgent>();
            carAgents[i].GetData(WheelDqn, MotorDqn);
        }
        
    }

    private void SaveNets()
    {
        FileModifier fileModifier = new FileModifier();
        fileModifier.WriteBinFile(WheelDqn.Policy, "WheelNet3.bin");
        fileModifier.WriteBinFile(MotorDqn.Policy, "MotorNet3.bin");
    }

    private void GetNets()
    {
        FileModifier fileModifier = new FileModifier();
        fileModifier.ReadNet(WheelDqn.Policy, "WheelNet2.bin");
        WheelDqn.UpdateTarget();
        fileModifier.ReadNet(MotorDqn.Policy, "MotorNet2.bin");
        MotorDqn.UpdateTarget();
    }
}
