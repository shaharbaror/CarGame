using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;
using Assets.Scripts.FileHandler;
using UnityEngine.UIElements;

public class DDPGTrainer : MonoBehaviour
{
    private DDPG _ddpg;

    // cars data
    [Header("Cars Data")]
    [Space(5)]
        public GameObject carPrefab;
        public int carCount = 1;
        public int maxActions = 5; // The maximum number of actions the car can take
        public float actionsPerSecond = 5; // The number of actions the car can take per second
        public float spawnRadius = 1f;

        private GameObject[] _cars;
        private CarActor[] _carActors;
        private CarRaycaster _carRaycaster;

    // layers architecture
    [Header("Layers Architecture")]
    [Space(5)]

    [SerializeField]
        private int _inputLayer;
        public int[] ActorLayers;
        public int[] CriticLayers;

    // hyperparameters
    [Header("Hyperparameters")]
    [Space(5)]

        public float actorLearningRate = 0.0003f;
        public float criticLearningRate = 0.001f;
        public float discountFactor = 0.98f;
        public float tau = 0.005f;
        public int replayMemorySize = 10000;
        public int batchSize = 128;
        public float noiseStdDev = 0.3f;

    public int minimumExperiences = 2000; 

    [Header("Noise Parameters")]
    [Space(5)]
        [SerializeField]
        private float _mu = 0.0f;

        [SerializeField]
        private float _theta = 0.15f;

        [SerializeField]
        private float _sigma = 0.3f;

        public float decayRate = 0.995f;


    [Header("Training Parameters")]
    [Space(5)]
    public int episodeCount;
    public int trainingCount;

    [Header("Debugging")]
    public int savingRate;

    private Dictionary<string, (int, float)> _hyperParameters;

    private void Start()
    {
        // connect to the car raycaster to get the amount of rays and other inputs
        _carRaycaster = carPrefab.GetComponent<CarRaycaster>();
        if (_carRaycaster == null)
        {
            Debug.LogError("CarRaycaster component not found on the car prefab.");
            return;
        }

        _inputLayer = _carRaycaster.GetInputSize();

        // create a dictionary to hold hyperparameters

        _hyperParameters = new Dictionary<string, (int, float)>
        {
            { "actorLearningRate", (0, actorLearningRate) },
            { "criticLearningRate", (0, criticLearningRate) },
            { "discountFactor", (0, discountFactor) },
            { "tau", (0, tau) },
            { "replayMemorySize", (replayMemorySize, 0) },
            { "batchSize", (batchSize, 0) },
            { "noiseStdDev", (0, noiseStdDev) },
            {"mu", (0, _mu) },
            {"theta", (0, _theta) },
            {"sigma", (0, _sigma) }
        };
        // create the DDPG model
        _ddpg = new DDPG(_inputLayer, ActorLayers, 2, CriticLayers, _hyperParameters);

        // initialize all the arrays that store the cars info
        _cars = new GameObject[carCount];
        _carActors = new CarActor[carCount];
        ReadFromFile();

        SpawnCars();
    }


    private void SpawnCars()
    {
        for (int i = 0; i < carCount; i++)
        {
            float xRange = Random.Range(-spawnRadius, spawnRadius);
            float zRange = Random.Range(-spawnRadius, spawnRadius);

            _cars[i] = Instantiate(carPrefab, new Vector3(transform.position.x + xRange, 0.2f, transform.position.z + zRange), Quaternion.Euler(0, 90, 0));

            _carActors[i] = _cars[i].GetComponent<CarActor>();
            Debug.Log(_hyperParameters);
            _carActors[i].Initialize(_inputLayer, ActorLayers, 2, _hyperParameters, maxActions, 1f / actionsPerSecond);
        }
    }


    public void Update()
    {
        bool allCarsDone = true;
        foreach (CarActor c in _carActors)
        {
            if (c.once)
            {
                allCarsDone = false;
                break;
            }
        }

        if (allCarsDone)
        {


            foreach (CarActor actor in _carActors)
            {
                
                    episodeCount++;

                    if (episodeCount % savingRate == 0)
                    {
                        // save the weights of the actor and critic networks
                        WriteToFiles();
                    }

                    if (episodeCount % 100 == 0 && noiseStdDev > 0.05)
                    {
                        // update the noiseStdDev
                        noiseStdDev = noiseStdDev * decayRate;
                    }

                    if (actor.Agent.Memory.Length() > minimumExperiences)
                    {
                        // get the batch of experiences
                        ContinuousNeuralState[] batch = actor.GetBatch();
                        trainingCount++;
                        // train the network on that batch
                        TrainNetwork(batch);
                    }

                    // transfer the new weights and noiseStd to the agents
                    actor.UpdateAgent(_ddpg.Actor, noiseStdDev);
                    actor.ResetCar();

                }
            
        }
    

        // add some training in the process
        


        if (Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale += 1;
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            Time.timeScale -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            Time.timeScale += 10;
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            Time.timeScale -= 10;
        }
    }


    private void TrainNetwork(ContinuousNeuralState[] batch)
    {
        // Train the DDPG network using the batch of experiences
        _ddpg.Train(batch);
    }

    private void WriteToFiles()
    {
        FileModifier modifier = new FileModifier();
        modifier.WriteBinFile(_ddpg.Actor.Policy, $"DDPG/4Actor-{(int)(episodeCount/savingRate)}.bin");
        modifier.WriteBinFile(_ddpg.Critic.Policy, $"DDPG/4Critic-{(int)(episodeCount/savingRate)}.bin");
    }

    private void ReadFromFile()
    {
        FileModifier modifier = new FileModifier();
        modifier.ReadNet(_ddpg.Actor.Policy, "DDPG/2Actor-18.bin");
        _ddpg.Actor.CopyTargetNetwork();
        modifier.ReadNet(_ddpg.Critic.Policy, "DDPG/2Critic-18.bin");
        _ddpg.Critic.CopyTargetNetwork();
    }
}
