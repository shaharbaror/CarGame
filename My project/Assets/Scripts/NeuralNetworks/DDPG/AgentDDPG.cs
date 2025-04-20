using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentDDPG
{
    private Actor _actor;
    private float _tau;


    public SpecialQueue Memory;
    private int _replayMemorySize;

    // Exploration noise parameters
    public float noiseStdDev;
    private OUNoise _ouNoise; // Ornstein-Uhlenbeck noise for better exploration

    public Actor Actor => _actor;


    public AgentDDPG(int stateSize, int[] actorHiddenLayers, int actionSize, Dictionary<string, (int, float)> hyperparameters)
    {
        // Extract hyperparameters
        this._tau = hyperparameters["tau"].Item2;
        this._replayMemorySize = hyperparameters["replayMemorySize"].Item1;
        this.noiseStdDev = hyperparameters["noiseStdDev"].Item2;
        // Initialize actor
        _actor = new Actor(stateSize, actorHiddenLayers, actionSize, _tau, hyperparameters["actorLearningRate"].Item2);

        Memory = new SpecialQueue(_replayMemorySize);

        // Initialize Ornstein-Uhlenbeck noise process
        _ouNoise = new OUNoise(actionSize, hyperparameters["mu"].Item2, hyperparameters["theta"].Item2, hyperparameters["sigma"].Item2 );
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
                action[i] += noise[i] * noiseStdDev;

                // Clip action to be within valid range of [-1, 1]
                action[i] = Mathf.Clamp(action[i], -1f, 1f);
            }
        }
        return action;
    }

    public void AddExperience(float[] state, float[] action, float reward, float[] nextState, bool done)
    {
        // Create a new experience tuple
        ContinuousNeuralState experience = new ContinuousNeuralState(state, action, reward, nextState, done);
        Memory.PushQueue(experience);
    }

    public ContinuousNeuralState[] GetBatch(int batchSize)
    {
        NeuralState[] batch = Memory.ClearAtRandom(batchSize);
        ContinuousNeuralState[] continouosBatch = new ContinuousNeuralState[batch.Length];
        for (int i = 0; i < batch.Length; i++)
        {
            continouosBatch[i] = (ContinuousNeuralState)batch[i];
        }
        return continouosBatch;
    }

    public void UpdateValues(Actor actor, float noiseChange)
    {
        // Update the actor network with the new values
        this.noiseStdDev = noiseChange;
        _actor.SetNetworks(actor);
    }
}
