using Unity.VisualScripting;
using UnityEngine;

public class Actor : DDPGNetwork
{
   
    public Actor(int stateSize, int[] hiddenLayers, int actionSize, float tau = 0.001f, float learningRate = 0.0005f) : base(tau, learningRate)
    {
        string[] activations = new string[hiddenLayers.Length + 1];

        // --------------- Need to add a Tanh activation function to the last layer of the actor network ----------------------

        for (int i =0; i < activations.Length; i++)
        {
            if (i == activations.Length - 1)
                activations[i] = "tanh"; // last layer activation function
            else
                activations[i] = "leakyrelu"; 

        }

        // Initialize the policy and target networks
        Policy = new NeuralNet(stateSize, hiddenLayers, actionSize, activations);
        Target = new NeuralNet(stateSize, hiddenLayers, actionSize, activations);
        // set the target network to be the same as the policy network
        Target.CloneNetwork(Policy);
    }


    // Get the action from the policy network
    public float[] GetAction(float[] state)
    {
        
        return Policy.ShowResults(state);
    }

    public float[] GetTargetAction(float[] state)
    {
        return Target.ShowResults(state);
    }

    public void UpdatePolicy(float[] state, float[] actionGradient)
    {
        // get the output of the policy network
        float[] actorOutput = this.GetAction(state);

        // get the output that we want to get to
        float[] targetOutput = new float[actorOutput.Length];

        // add the gradient to the output for the desired action
        for (int i=0; i < actorOutput.Length; i++)
        {
            targetOutput[i] = actorOutput[i] + _learningRate * actionGradient[i];   
            targetOutput[i] = Mathf.Clamp(targetOutput[i], -1f, 1f); // clip the action to be between -1 and 1
        }

        Policy.Backpropagate(state, targetOutput, _learningRate);


    }

}
