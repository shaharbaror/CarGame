
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

class DDPG
{
    // Networks
    private Actor _actor;
    private Critic _critic;

    private SpecialQueue _Memory;

    // Hyperparameters
    //private float _actorLearningRate;
    //private float _criticLearningRate;
    private float _discountFactor;
    private float _tau;
    private int _replayMemorySize;
    private int _batchSize;

    // Exploration noise parameters
    private float _noiseStdDev;
    private System.Random _random;
    private OUNoise _ouNoise; // Ornstein-Uhlenbeck noise for better exploration

    public Actor Actor => _actor;
    public Critic Critic => _critic;

    public DDPG(int stateSize, int[] actorHiddenLayers, int actionSize,
        int[] criticHiddenLayers, Dictionary<string, (int, float)> hyperparameters)
    {
        //Extract hyperparameters
        //this._actorLearningRate = hyperparameters["actorLearningRate"].Item2;
        //this._criticLearningRate = hyperparameters["criticLearningRate"].Item2;
        this._discountFactor = hyperparameters["discountFactor"].Item2;
        this._tau = hyperparameters["tau"].Item2;
        this._replayMemorySize = hyperparameters["replayMemorySize"].Item1;
        this._batchSize = hyperparameters["batchSize"].Item1;
        this._noiseStdDev = hyperparameters["noiseStdDev"].Item2;

        // Initialize actor and critic
        _actor = new Actor(stateSize, actorHiddenLayers, actionSize, _tau, hyperparameters["actorLearningRate"].Item2);
        _critic = new Critic(stateSize, actionSize, criticHiddenLayers, _tau, hyperparameters["criticLearningRate"].Item2);

        // Initialize replay memory
        _Memory = new SpecialQueue(_replayMemorySize);

        // Initialize random number generator for noise
        _random = new System.Random();

        // Initialize Ornstein-Uhlenbeck noise process
        _ouNoise = new OUNoise(actionSize, hyperparameters["mu"].Item2, hyperparameters["theta"].Item2, hyperparameters["sigma"].Item2);
    }

    public float[] GetAction(float[] state, bool addNoise = true)
    {
        // Get a deterministic action from the actor network
        float[] action = _actor.GetAction(state);

        if (addNoise)
        {
            // Add exploration noise to the action
            float[] noise = _ouNoise.Sample();
            for (int i = 0; i < action.Length; i++)
            {
                action[i] += noise[i]*_noiseStdDev;

                // Clip action to be within valid range of [-1, 1]
                action[i] = Mathf.Clamp(action[i], -1f, 1f);
            }
        }
        return action;
    }

    // Add experience to replay memory
    public void AddExperience(float[] state, float[] action, float reward, float[] nextState, bool done)
    {
        // Create a new experience tuple
        ContinuousNeuralState experience = new ContinuousNeuralState(state, action, reward, nextState, done);
        _Memory.PushQueue(experience);
    }


    // Train the actor and critic networks using a batch of experiences from replay memory
    public void Train()
    {
        if (_Memory.Length() < _batchSize)
        {
            return; // Not enough experiences to sample from
        }

        // Get a batch of experiences from replay memory
        NeuralState[] batchStates = _Memory.ClearAtRandom(_batchSize);


        // Convert the batch of experiences to a ContinuousNeuralState array
        ContinuousNeuralState[] batch = new ContinuousNeuralState[_batchSize];
        for (int i =0; i < _batchSize; i++)
        {
            batch[i] = (ContinuousNeuralState)batchStates[i];
        }

        TrainCritic(batch);

        TrainActor(batch);

        // Update target networks
        _actor.SoftUpdateTarget();
        _critic.SoftUpdateTarget();
    }

    private void TrainCritic(ContinuousNeuralState[] batch)
    {
        foreach (ContinuousNeuralState experience in batch)
        {
            if (experience == null || experience.newState == null)
            {
                continue; // Skip null experiences
            }

            // Get the target Q value 
            float targetQ = experience.reward;

            if (!experience.terminated)
            {
                /// Get the target Q value from the target critic network
                /// get the target Q-value from the target critic network
                /// calculate the target Q-value using the Bellman equation
                float[] nextAction = _actor.GetTargetAction(experience.newState);
                targetQ += _discountFactor * _critic.GetTargetQValue(experience.newState, nextAction);

            }


            // Get the current Q value from the critic network
            float currentQ = _critic.GetQValue(experience.state, experience.action);

            _critic.TrainPolicy(experience.state, experience.action, targetQ); // Train the critic network using the input and target Q value
        }
    }

    private void TrainActor(ContinuousNeuralState[] batch)
    {
        foreach (ContinuousNeuralState experience in batch)
        {
            if (experience == null || experience.newState == null)
            {
                continue; // Skip null experiences
            }

            // Get current action from the actor network
            float[] action = _actor.GetAction(experience.state);

            // Calculate the gradient of the Q value with respect to the action
            float[] actionGradient = _critic.GetActionGradient(experience.state, action);

            _actor.UpdatePolicy(experience.state, actionGradient);
        }
    }


   




}

