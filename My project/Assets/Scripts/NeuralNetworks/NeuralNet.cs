using UnityEngine;
using System.Collections.Generic;
public class NeuralNet
{
    public List<Layer> layers;
    public float[] InputLayer, OutputLayer;
    
    public NeuralNet(int inputLayerSize,int[] layerSizes, int outputLayer, string[] activations)
    {

        layers = new List<Layer>();

        // creatre the rest of the layers 
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            layers.Add(new Layer(i == 0? inputLayerSize:layerSizes[i - 1], layerSizes[i], activations[i]));
        }

        // creater an output layer
        layers.Add(new Layer(layerSizes[layerSizes.Length - 2], outputLayer, activations[activations.Length-1]));
    }

    // go through every layer and feed forward the inputs untill
    // the final one
    public float[] ShowResults(float[] inputs)
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
    public float ComputeLoss(float[] predicted, float[] target)
    {
        float loss = 0;
        for (int i = 0; i < predicted.Length; i++)
        {
            loss += Mathf.Pow(predicted[i] - target[i], 2);
        }
        return loss;
    }


    public void Backpropagate(float[] input, float[] targetOutput, float learningRate)
    {
        // Forward pass
        float[] predictedOutput = this.ShowResults(input);

        // Calculate output layer error
        float[] outputError = new float[targetOutput.Length];
        for (int i = 0; i < targetOutput.Length; i++)
        {
            outputError[i] = predictedOutput[i] - targetOutput[i];
        }

        // Backward pass
        float[] error = outputError;
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            Layer layer = layers[i];
            float[] newError = new float[layer.InputSize];
            float[,] weightGradients = new float[layer.NeuronCount, layer.InputSize];
            float[] biasGradients = new float[layer.NeuronCount];

            for (int j = 0; j < layer.NeuronCount; j++)
            {
                float delta = error[j] * layer.Activation.Derivative(layer.LastValues[j]); // Sigmoid or ReLU derivative
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

    // Soft update the target network weights and biases using a factor tau
    public void SoftUpdate(NeuralNet other, float tau)
    {
        if (other.layers.Count != this.layers.Count)
        {
            Debug.LogError("Networks have different number of layers.");
            return;
        }
        // Update the target weights and biases using soft update
        for (int i = 0; i < layers.Count; i++)
        {
            Layer thisLayer = layers[i];
            Layer otherLayer = other.layers[i];
            for (int j = 0; j < thisLayer.NeuronCount; j++)
            {
                // update the biases
                thisLayer.Biases[j] = (1 - tau) * thisLayer.Biases[j] + tau * otherLayer.Biases[j];

                // update the weights
                for (int k = 0; k < thisLayer.InputSize; k++)
                {
                    thisLayer.Weights[j, k] = (1 - tau) * thisLayer.Weights[j, k] + tau * otherLayer.Weights[j, k];
                }
            }
        }
    }

    public void CloneNetwork(NeuralNet other)
    {
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].SetLayer(other.layers[i].Weights, other.layers[i].Biases);
        }
    }


    // -------------------------------- Should basically not exist -------------------
    public int GetLayerCount()
    {
        return this.layers.Count;
    }

    public void SetLayerIndex(int index, float[,] weights, float[] biases)
    {
        layers[index].SetLayer(weights, biases);
    }

    public float[,] GetLayerIndexWeights(int index)
    {
        return layers[index].Weights;
    }

    public float[] GetLayerIndexBiases(int index)
    {
        return layers[index].Biases;
    }


    public Layer GetLayerAtIndex(int index)
    {
        return layers[index];
    }

   



}
