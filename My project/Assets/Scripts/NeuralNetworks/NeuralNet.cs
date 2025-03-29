using UnityEngine;
using System.Collections.Generic;
public class NeuralNet
{
    private List<Layer> layers;
    private double[] InputLayer, OutputLayer;
    
    public NeuralNet(int inputLayerSize,int[] layerSizes, int outputLayer)
    {

        layers = new List<Layer>();

        // creatre the rest of the layers 
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            layers.Add(new Layer(i == 0? inputLayerSize:layerSizes[i - 1], layerSizes[i]));
        }

        // creater an output layer
        layers.Add(new Layer(layerSizes[layerSizes.Length - 2], outputLayer, "sigmoid"));
    }

    // go through every layer and feed forward the inputs untill
    // the final one
    public double[] ShowResults(double[] inputs)
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


    public void Backpropagate(double[] input, double[] targetOutput, double learningRate)
    {
        // Forward pass
        double[] predictedOutput = this.ShowResults(input);

        // Calculate output layer error
        double[] outputError = new double[targetOutput.Length];
        for (int i = 0; i < targetOutput.Length; i++)
        {
            outputError[i] = predictedOutput[i] - targetOutput[i];
        }

        // Backward pass
        double[] error = outputError;
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            Layer layer = layers[i];
            double[] newError = new double[layer.InputSize];
            double[,] weightGradients = new double[layer.NeuronCount, layer.InputSize];
            double[] biasGradients = new double[layer.NeuronCount];

            for (int j = 0; j < layer.NeuronCount; j++)
            {
                double delta = error[j] * layer.Activation.Derivative(layer.LastValues[j]); // Sigmoid or ReLU derivative
                biasGradients[j] = delta;

                for (int k = 0; k < layer.InputSize; k++)
                {
                    weightGradients[j, k] = delta * layer.LastInputs[k];
                    newError[k] += delta * layer.Weights[j, k];
                }
            }

            // Update weights and biases
            for (int j = 0; j < layer.NeuronCount; j++)
            {
                layer.Biases[j] -= learningRate * biasGradients[j];
                for (int k = 0; k < layer.InputSize; k++)
                {
                    layer.Weights[j, k] -= learningRate * weightGradients[j, k];
                }
            }

            error = newError;
        }
    }

    public int GetLayerCount()
    {
        return this.layers.Count;
    }

    public void SetLayerIndex(int index, double[,] weights, double[] biases)
    {
        layers[index].SetLayer(weights, biases);
    }

    public double[,] GetLayerIndexWeights(int index)
    {
        return layers[index].Weights;
    }

    public double[] GetLayerIndexBiases(int index)
    {
        return layers[index].Biases;
    }



}
