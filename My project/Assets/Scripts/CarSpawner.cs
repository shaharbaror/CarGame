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
    public int savingRate = 1000;

    public int maxActions = 5; // The maximum number of actions the car can take



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
            // create a dictionary to hold the values that are going to be changed in the agents
            Dictionary<string, int> changes = new Dictionary<string, int> {
            { "replayMemorySize", replayMemorySize },
            { "batchSize", (int)(batchSize/carCount)},
            { "maxActions", maxActions}};
        SetTheAgents(changes);


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

            if (EpisodeCount % 500 == 0)
            {
                epsilon = Mathf.Lerp(0.5f, 0.01f, (float)EpisodeCount / 100000);
                WheelDqn.ChangeEpsilon(epsilon);
                MotorDqn.ChangeEpsilon(epsilon);
            }
            if (EpisodeCount % savingRate == 0)
            {
                SaveNets();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 100;

            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Time.timeScale = 1;
            }
            else if (Input.GetKeyDown(KeyCode.L)) {
                foreach (CarAgent c in carAgents)
                {
                    c.ChangeMaxActions(maxActions);
                }
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
        //for (int i = 0; i < carCount; i++)
        //{
        //    cars[i] = Instantiate(carPrefab, new Vector3(transform.position.x, 0.2f, transform.position.z), Quaternion.Euler(0,90,0));
        //    carAgents[i] = cars[i].GetComponent<CarAgent>();
        //    carAgents[i].GetData(WheelDqn, MotorDqn);
        //}
        cars[0] = Instantiate(carPrefab, new Vector3(transform.position.x, 0.2f, transform.position.z), Quaternion.Euler(0, 110, 0));
        carAgents[0] = cars[0].GetComponent<CarAgent>();
        carAgents[0].GetData(WheelDqn, MotorDqn);

        //cars[1] = Instantiate(carPrefab, new Vector3(transform.position.x, 0.2f, 63f), Quaternion.Euler(0, -90, 0));
        //carAgents[1] = cars[1].GetComponent<CarAgent>();
        //carAgents[1].GetData(WheelDqn, MotorDqn);

    }

    private void SaveNets()
    {
        FileModifier fileModifier = new FileModifier();
        int i = EpisodeCount / savingRate;
        fileModifier.WriteBinFile(WheelDqn.Policy, $"WheelNet2{i}.bin");
        fileModifier.WriteBinFile(MotorDqn.Policy, $"MotorNet2{i}.bin");
    }

    private void SetTheAgents(Dictionary<string, int> changes)
    {
        for (int i = 0; i < carCount; i++)
        {
            carAgents[i].SetAgent(changes);
        }

    }

    private void GetNets()
    {
        FileModifier fileModifier = new FileModifier();
        fileModifier.ReadNet(WheelDqn.Policy, "/WheelNet11.bin");
        WheelDqn.UpdateTarget();
        fileModifier.ReadNet(MotorDqn.Policy, "/MotorNet11.bin");
        MotorDqn.UpdateTarget();
    }
}
