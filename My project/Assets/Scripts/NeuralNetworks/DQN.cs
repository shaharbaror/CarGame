using System.Collections.Generic;
using UnityEngine;


public class DQN
{
    public NeuralNet Policy, Target;
    private SpecialQueue Memory;

    private float learningRate, discountFactor;
    private int networkSyncRate, replayMemorySize, batchSize;

    // initialization
    public DQN(int inputLayer, int[] hiddenLayer, int outputLayer, Dictionary<string, (int, float)> parameters)
    {
        Policy = new NeuralNet(inputLayer, hiddenLayer, outputLayer);
        Target = new NeuralNet(inputLayer, hiddenLayer, outputLayer);

        CopyNetworks(Policy, Target);

        this.SetMainValues(parameters);
        this.Memory = new SpecialQueue(this.replayMemorySize);
    }



    // create an Experiance Replay buffer to hold the Neural Network's experiances
    public void SetMemory(int maxNumber)
    {
        this.replayMemorySize = maxNumber;
        Memory.ChangeMaxSize(maxNumber);
    }

    // setting up the Main Values considering the Dictionary does hold every value
    public void SetMainValues(Dictionary<string, (int, float)> p)
    {
        this.learningRate = p["learningRate"].Item2;
        this.discountFactor = p["discountFactor"].Item2;
        this.networkSyncRate = p["netwrokSyncRate"].Item1;
        this.replayMemorySize = p["replayMemorySize"].Item1;
        this.batchSize = p["batchSize"].Item1;
    }

    private void CopyNetworks(NeuralNet p, NeuralNet t)
    {         // update the target network with the policy network
        for (int i = 0; i < p.GetLayerCount(); i++)
        {
            // sets the weights and biases from Policy to Target
            t.SetLayerIndex(i, p.GetLayerIndexWeights(i), p.GetLayerIndexBiases(i));
        }
    }

    public void UpdateTarget()
    {
        CopyNetworks(Policy, Target);
    }

    // TODO: need to do this function -------------------------------------------------------------------------
    //public double[] GetState()
    //{
    //    // gets some type of state
    //    double[] doubles = new double[1];
    //    return doubles;
    //}

    // TODO: need to do this function -------------------------------------------------------------------------
    //public (double[], double, bool, bool) ApplyAction(int action)
    //{
    //    // do something something to the car Agent and get the values for it





    //    // apply the action to the state
    //    double[] newState = GetState();
    //    double reward = 0;
    //    bool terminated = false;
    //    bool success = false;
    //    return (newState, reward, terminated, success);
    //}

    // gets a state and returns an action
    public int GetAction(float[] state)
    {
        float[] options = this.Policy.ShowResults(state);
        return PreformEpsilonGreedy(options);
    }

    /*

    public void Train(int episodes)
    {
        int stepCount = 0;          // used to keep track of the steps taken for synchronizing the networks
        int rewardsCount = 0;       // used to keep track of the rewards gained

        double[] curState = null;
        double[] newState = GetState(); // the input from the 'next' state
        double[] options = null;
        int action;
        double reward;
        bool terminated, truncated;

        options = this.Policy.ShowResults(newState);

        // do the action
        action = PreformEpsilonGreedy(options);     // choose an action to do based on an Epsilon Greedy approach

        // move the new state to the current state value
        curState = newState;
        (newState, reward, terminated, truncated) = ApplyAction(action);    // currently this doesnt realy do anythin but for now we do this


        // add the experiance to the memory
        this.Memory.Push(new NeuralState(curState, action, reward, newState, terminated));

        for (int i = 0; i < episodes; i++)
        {
            terminated = false;
            truncated = false;      // will activate after a set number of steps
            rewardsCount = 0;
            while (!terminated && !truncated)
            {
                // get the options from the policy network
                options = this.Policy.ShowResults(newState);
                // choose an action to do based on an Epsilon Greedy approach
                action = PreformEpsilonGreedy(options);
                // apply the action in the environment and get the resaults
                curState = newState;
                (newState, reward, terminated, truncated) = ApplyAction(action);

                // add the experiance to the memory
                this.Memory.Push(new NeuralState(curState, action, reward, newState, terminated));

                stepCount++;
                if (reward > 0) rewardsCount++;
            }
            


            // check if there is enough memory to train the network
            if (this.Memory.Length() > this.batchSize && rewardsCount > 0)
            {
                // get a random batch of experiances from the memory
                NeuralState[] batch = this.Memory.ClearAtRandom(this.batchSize);
                // train the network with the batch
                    // TODO: create the Train the network function
                    TeachTheNetwork(batch);
            }

            // update the target network with the policy network
            if (stepCount > this.networkSyncRate)
            {
                UpdateTarget();
                stepCount = 0;
            }
        }



    }
    */

    // train the network with a batch of experiances
    //public void TeachTheNetwork(NeuralState[] batch)
    //{
    //    // get the target values for the batch
    //    float[] targetValues = new float[batch.Length];
    //    float[] predictedValues = new float[batch.Length];
    //    for (int i = 0; i < batch.Length; i++)
    //    {
    //        targetValues[i] = batch[i].reward;
    //        if (!batch[i].terminated)
    //        {
    //            float[] options = this.Target.ShowResults(batch[i].newState);
    //            targetValues[i] += this.discountFactor * options[PreformEpsilonGreedy(options)];
    //        }
    //        predictedValues[i] = this.Policy.ShowResults(batch[i].state)[batch[i].action];
    //    }
    //    // calculate the loss of the network
    //    float loss = this.Policy.ComputeLoss(predictedValues, targetValues);
    //    // backpropagate the loss
    //    this.Policy.Backpropagate(batch[0].state, targetValues, this.learningRate);
    //}

    public void TeachTheNetwork(NeuralState[] batchs)
    {
        //Debug.Log("Teaching the Network");
        foreach (NeuralState batch in batchs)
        {
            float targetValue = 0;
            if (!batch.terminated)
            {
                float[] options = this.Target.ShowResults(batch.newState);
                targetValue = batch.reward + this.discountFactor * options[PreformEpsilonGreedy(options)];
            }
            float[] currentQValues = this.Policy.ShowResults(batch.state);

            float[] targetQValues = (float[])currentQValues.Clone();
            targetQValues[batch.action] = targetValue;
            for (int i = 0; i < targetQValues.Length; i++)
            {
                Debug.Log(targetQValues[i] - currentQValues[i]);
            }

            this.Policy.Backpropagate(batch.state, targetQValues, this.learningRate);
        }
    }



    private int PreformEpsilonGreedy(float[] options) { 
        if (Random.Range(0f, 1f) > 0.5f)
        {
            // preform the best action
            int maximum = 0;
            for (int i =1; i< options.Length; i++)
            {
                if (options[i] > options[maximum]) maximum = i;

            }
            return maximum;
        }
        return Random.Range(0, options.Length);

    }

    public void Clone(DQN dqn)
    {
        CopyNetworks(dqn.Policy, this.Policy);
        CopyNetworks(dqn.Target, this.Target);
        this.learningRate = dqn.learningRate;
        this.discountFactor = dqn.discountFactor;
        this.networkSyncRate = dqn.networkSyncRate;
        this.replayMemorySize = dqn.replayMemorySize;
        this.batchSize = dqn.batchSize;

    }


}
