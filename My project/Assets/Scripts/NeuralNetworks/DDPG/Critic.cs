using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Critic : DDPGNetwork
{

    public Critic(int inputSize, int actionSize, int[] hiddenLayers, float tau = 0.001f, float learningRate = 0.0005f) : base(tau, learningRate)
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
        float[] input = CombineStateAction(state, action);

        // Get the Q-value from the policy network
        return Policy.ShowResults(input)[0];
    }

    // Get Q-value from the target critic network
    public float GetTargetQValue(float[] state, float[] action)
    {
        // Combine the state and action into a single input array
        float[] input = CombineStateAction(state, action);
        // Get the Q-value from the target network
        return Target.ShowResults(input)[0];
    }

    public float[] CombineStateAction(float[] state, float[] action)
    {

        float[] input = new float[state.Length + action.Length];
        System.Array.Copy(state, input, state.Length);
        System.Array.Copy(action, 0, input, state.Length, action.Length);
        return input;
    }

    // Train the policy network using the state, action, and target Q-value
    public void TrainPolicy(float[] state, float[] action, float targetQ)
    {
        // Combine the state and action into a single input array
        float[] input = CombineStateAction(state, action);
        // Create a target array with the target Q-value
        float[] targetArr = new float[] { targetQ };
        
        Policy.Backpropagate(input, targetArr, _learningRate);
    }

    public float[] GetActionGradient(float[] state, float[] action, float delta = 0.0001f)
    {
        // Combine the state and action into a single input array
        float[] input = CombineStateAction(state, action);



        // initialize the gradient array
        // go through every action
        // for every action compute the Q value for lowering the action
        // and compute for raising the action
        // get the difference and divide by 2
        // clip the gradient to be between -1 and 1     -- optional

        float[] gradient = new float[action.Length];

        for (int  i = 0;  i < action.Length;  i++)
        {
            float[] incAction = (float[])action.Clone();
            float[] decAction = (float[])action.Clone();

            incAction[i] += delta;
            decAction[i] -= delta;

            incAction[i] = Mathf.Clamp(incAction[i], -1f, 1f);
            decAction[i] = Mathf.Clamp(decAction[i], -1f, 1f);

            float incQ = this.GetQValue(state, incAction);
            float decQ = this.GetQValue(state, decAction);

            gradient[i] = (incQ - decQ) / (2 * delta);
        }

        return gradient;
    }

    


}
