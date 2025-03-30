using System.Collections.Generic;
using UnityEngine;


public class DQN
{
    private NeuralNet Policy, Target;
    private SpecialQueue Memory;

    private double learningRate, discountFactor;
    private int networkSyncRate, replayMemorySize, batchSize;

    // initialization
    public DQN(int inputLayer, int[] hiddenLayer, int outputLayer, Dictionary<string, (int, double)> parameters)
    {
        Policy = new NeuralNet(inputLayer, hiddenLayer, outputLayer);
        Target = new NeuralNet(inputLayer, hiddenLayer, outputLayer);

        UpdateTarget();

        this.SetMainValues(parameters);
        this.Memory = new SpecialQueue(this.replayMemorySize);
    }



    // create an Experiance Replay buffer to hold the Neural Network's experiances
    public void SetMemory(int maxNumber)
    {
        this.replayMemorySize = maxNumber;
        Memory = new SpecialQueue(maxNumber);
    }

    // setting up the Main Values considering the Dictionary does hold every value
    public void SetMainValues(Dictionary<string, (int, double)> p)
    {
        this.learningRate = p["learningRate"].Item2;
        this.discountFactor = p["discountFactor"].Item2;
        this.networkSyncRate = p["netwrokSyncRate"].Item1;
        this.replayMemorySize = p["replayMemorySize"].Item1;
        this.batchSize = p["batchSize"].Item1;
    }

    private void UpdateTarget()
    {         // update the target network with the policy network
        for (int i = 0; i < Policy.GetLayerCount(); i++)
        {
            // sets the weights and biases from Policy to Target
            Target.SetLayerIndex(i, Policy.GetLayerIndexWeights(i), Policy.GetLayerIndexBiases(i));
        }
    }


}
