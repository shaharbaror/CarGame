
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

class DDPG
{
    // Networks
    private Actor _actor;
    private Critic _critic;

    private SpecialQueue _Memory;

    // Hyperparameters
    private float _actorLearningRate;
    private float _criticLearningRate;
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
        this._actorLearningRate = hyperparameters["actorLearningRate"].Item2;
        this._criticLearningRate = hyperparameters["criticLearningRate"].Item2;
        this._discountFactor = hyperparameters["discountFactor"].Item2;
        this._tau = hyperparameters["tau"].Item2;
        this._replayMemorySize = hyperparameters["replayMemorySize"].Item1;
        this._batchSize = hyperparameters["batchSize"].Item1;
        this._noiseStdDev = hyperparameters["noiseStdDev"].Item2;

        // Initialize actor and critic
        _actor = new Actor(stateSize, actorHiddenLayers, actionSize, _tau);
        _critic = new Critic(stateSize, actionSize, criticHiddenLayers, _tau);

        // Initialize replay memory
        _Memory = new SpecialQueue(_replayMemorySize);

        // Initialize random number generator for noise
        _random = new System.Random();

        // Initialize Ornstein-Uhlenbeck noise process
        _ouNoise = new OUNoise(actionSize, 0.15f, 1.0f, 0.2f);
    }

}

