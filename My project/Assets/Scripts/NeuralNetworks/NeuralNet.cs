using UnityEngine;
using System.Collections.Generic;
public class NeuralNet : MonoBehaviour
{
    private List<Layer> layers;
    private double[] InputLayer, OutputLayer;

    private void Awake()
    {
        layers = new List<Layer>();
    }
    
    public NeuralNet(int[] layerSizes)
    {

        layers = new List<Layer>();

        // creatre the rest of the layers 
        for (int i = 1; i < layerSizes.Length - 1; i++)
        {
            layers.Add(new Layer(layerSizes[i - 1], layerSizes[i]));
        }

        // creater an output layer
        layers.Add(new Layer(layerSizes[layerSizes.Length - 2], layerSizes[layerSizes.Length - 1], "sigmoid"));
    }

    // go through every layer and feed forward the inputs untill
    // the final one
    public double[] showResults(double[] inputs)
    {

        // save the input and output layers for the back probagation
        InputLayer = inputs;
        OutputLayer = inputs;
        foreach (Layer layer in layers)
        {
            OutputLayer = layer.FeedForward(OutputLayer);
        }

        // returns the final values of the output layer
        return OutputLayer;
    }


    // compute the loss of the network
    public double ComputeLoss(double[] predicted, double[] target)
    {
        double loss = 0;
        for (int i = 0; i < predicted.Length; i++)
        {
            loss += Mathf.Pow((float)(predicted[i] - target[i]), 2);
        }
        return loss;
    }

    public void Backpropagate(double[] input, double[] targetOutput)
    {
        
    }


}
