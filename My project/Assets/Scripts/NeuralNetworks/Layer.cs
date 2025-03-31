using UnityEngine;
using Assets.Scripts.NeuralNetworks.Activations;
using Assets.Scripts.NeuralNetworks.WeightInits;

public class Layer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float[,] Weights;
    public float[] Biases;
    public float[] LastValues;                 // contains the values from the last feed forward
    public float[] LastInputs;
    public int InputSize, NeuronCount;

    public ActivationFunction Activation { get; private set; }
    public WeightInitializer _WeightInitializer { get; private set; }


    public Layer(int inputSize, int neuronCount, string activation = "relu")
    {
        this.InputSize = inputSize;
        this.NeuronCount = neuronCount;

        // if no activation function is provided, use ReLU
        this.Activation = ActivationUtility.GetActivationFunction(activation);
        // get the default initializer based on the activation funciton
        this._WeightInitializer = WeightUtility.GetDefaultInitializer(activation);

        // if the initializer if of a xavierInitializer type
        if (this._WeightInitializer is XavierInitializer xavier)
        {
            xavier.setLayerSize(neuronCount);
        }


        Weights = new float[neuronCount, inputSize];
        Biases = new float[neuronCount];
        LastValues = new float[neuronCount];

        InitializeLayer();
    }

    private void InitializeLayer()
    {
        float[] neuronWeights;
        for (int i = 0; i < NeuronCount; i++) {
            // use the initializer to generate the bias
            Biases[i] = this._WeightInitializer.GenerateBias();

            // use the initializer to generate the weights for the neuron
            neuronWeights = this._WeightInitializer.GenerateWeights(InputSize);

            for (int j = 0; j < InputSize; j++)
            {
                // assign the weight for the neuron
                Weights[i, j] = neuronWeights[j];
            }
        }
    }

    public float[] FeedForward(float[] inputs)
    {
        this.LastInputs = inputs;
        float[] outputs = new float[NeuronCount];
        for (int i = 0; i < NeuronCount; i++)
        {
            // add the bias to the sum
            float sum = Biases[i];

            for (int j = 0; j < InputSize; j++)
            {
                // add the weight * input to the sum
                sum += Weights[i, j] * inputs[j];
            }
            // apply the activation function to the sum
            outputs[i] = this.Activation.Activate(sum);
            // insert the output into the last values
            LastValues[i] = outputs[i];
        }
        return outputs;
    }

    public void SetLayer(float[,] weights, float[] biases)
    {
        // make sure we can set the layer
        if (weights.GetLength(1) != this.Weights.GetLength(1) || weights.GetLength(0) != this.Weights.GetLength(0))
            throw new System.Exception("Cannot Copy weights, Weights of different sizes");
        if (biases.Length != this.Biases.Length)
            throw new System.Exception("Cannot Copy weights, Biases of different sizes");

        for (int i = 0; i < weights.GetLength(0); i++)
        {
            for (int j = 0; j < weights.GetLength(1); j++)
            {
                this.Weights[i, j] = weights[i, j];
            }
            this.Biases[i] = biases[i];
        }
    
    }

    public int GetNeuronCount() { return NeuronCount; }
}
