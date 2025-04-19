using UnityEngine;

public class Critic : DDPGNetwork
{

    public Critic(int inputSize, int actionSize, int[] hiddenLayers, float tau = 0.001f) : base(tau)
    {
        
        string[] activations = new string[hiddenLayers.Length + 1];
        for (int i = 0; i < activations.Length; i++)
        {
            if (i == activations.Length - 1)
                activations[i] = "none"; // last layer activation function
            else
                activations[i] = "leakyrelu";

        }

        // For the critic, the input size is the state size + action size
        // This is because the critic takes both the state and action as input and returns the apropriate Q-value based on them

        Policy = new NeuralNet(inputSize + actionSize, hiddenLayers, 1, activations);        
        Target = new NeuralNet(inputSize + actionSize, hiddenLayers, 1, activations);

        // set the target network to be the same as the policy network
        Target.CloneNetwork(Policy);
    }

    // Get Q-value from the critic network
    public float GetQValue(float[] state, float[] action)
    {
        // Combine the state and action into a single input array
        float[] input = new float[state.Length + action.Length];
        System.Array.Copy(state, input, state.Length);
        System.Array.Copy(action, 0, input, state.Length, action.Length);

        // Get the Q-value from the policy network
        return Policy.ShowResults(input)[0];
    }

    // Get Q-value from the target critic network
    public float GetTargetQValue(float[] state, float[] action)
    {
        // Combine the state and action into a single input array
        float[] input = new float[state.Length + action.Length];
        System.Array.Copy(state, input, state.Length);
        System.Array.Copy(action, 0, input, state.Length, action.Length);
        // Get the Q-value from the target network
        return Target.ShowResults(input)[0];
    }


}
