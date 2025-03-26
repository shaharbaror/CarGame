using UnityEngine;
using Assets.Scripts.NeuralNetworks.Activations;
using Assets.Scripts.NeuralNetworks.WeightInits;

public class Layer
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public double[,] Weights;
    public double[] Biases;
    public double[] LastValues;                 // contains the values from the last feed forward
    public double[] LastInputs;
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


        Weights = new double[neuronCount, inputSize];
        Biases = new double[neuronCount];
        LastValues = new double[neuronCount];

        InitializeLayer();
    }

    private void InitializeLayer()
    {
        double[] neuronWeights;
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

    public double[] FeedForward(double[] inputs)
    {
        this.LastInputs = inputs;
        double[] outputs = new double[NeuronCount];
        for (int i = 0; i < NeuronCount; i++)
        {
            double sum = Biases[i];
            for (int j = 0; j < InputSize; j++)
            {
                sum += Weights[i, j] * inputs[j];
            }
            outputs[i] = this.Activation.Activate(sum);
            // insert the output into the last values
            LastValues[i] = outputs[i];
        }
        return outputs;
    }
}
