using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;

public class DDPGTrainer : MonoBehaviour
{
    private DDPG _ddpg;

    // cars data
    [Header("Cars Data")]
    [Space(5)]
        public GameObject carPrefab;
        public int carCount = 1;
        public float maxActions = 5; // The maximum number of actions the car can take
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

        public float actorLearningRate = 0.001f;
        public float criticLearningRate = 0.001f;
        public float discountFactor = 0.99f;
        public float tau = 0.01f;
        public int replayMemorySize = 10000;
        public int batchSize = 64;
        public float noiseStdDev = 0.2f;

    [Header("Noise Parameters")]
    [Space(5)]
        [SerializeField]
        private float _mu = 0.15f;

        [SerializeField]
        private float _theta = 1.0f;

        [SerializeField]
        private float _sigma = 0.2f;

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
            _carActors[i].Initialize(_inputLayer, ActorLayers, 2, _hyperParameters);
        }
    }


    public void Update()
    {
        bool areDone = true;
        foreach (CarActor actor in _carActors) 
        {
            if (actor.sessionRunning)
            {
                areDone = false;
                break;
            }
        }

        // add some training in the process

        if (areDone)
        {
            foreach (CarActor actor in _carActors)
            {
                actor.ResetCar();
            }
        }
    }
}
