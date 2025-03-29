using UnityEngine;


public class DQN
{
    private NeuralNet Policy, Target;

    public DQN(int inputLayer, int[] hiddenLayer, int outputLayer)
    {
        Policy = new NeuralNet(inputLayer, hiddenLayer, outputLayer);
        Target = new NeuralNet(inputLayer, hiddenLayer, outputLayer);
        UpdateTarget();
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
