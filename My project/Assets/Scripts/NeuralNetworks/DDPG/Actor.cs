using Unity.VisualScripting;
using UnityEngine;

public class Actor : DDPGNetwork
{
   
    public Actor(int stateSize, int[] hiddenLayers, int actionSize, float tau = 0.001f) : base(tau)
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

}
