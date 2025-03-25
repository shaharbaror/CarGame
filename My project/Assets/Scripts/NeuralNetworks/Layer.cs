using UnityEngine;

public class Layer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public double[,] Weights;
    public double[] Biases;
    public double[] LastValues;                 // contains the values from the last feed forward
    public int InputSize, NeuronCount;
    public string ActivationFunction;


    public Layer(int inputSize, int neuronCount, string activationFunction="relu")
    {
        this.InputSize = inputSize;
        this.NeuronCount = neuronCount;
        this.ActivationFunction = activationFunction;

        Weights = new double[neuronCount, inputSize];
        Biases = new double[neuronCount];
        LastValues = new double[neuronCount];

        InitializeLayer();
        ActivationFunction = activationFunction;
    }

    private void InitializeLayer()
    {
        for (int i =0; i < NeuronCount; i++) {
            Biases[i] = 0;
            for (int j = 0; j < InputSize; j++)
            {
                Weights[i, j] = Random.Range(0, 1f);
            }
        }
    }

    public double[] FeedForward(double[] inputs)
    {
        double[] outputs = new double[NeuronCount];
        for (int i = 0; i < NeuronCount; i++)
        {
            double sum = Biases[i];
            for (int j = 0; j < InputSize; j++)
            {
                sum += Weights[i, j] * inputs[j];
            }
            outputs[i] = this.ActivationFunction == "relu" ? ReLU(sum) : Sigmoid(sum);
            // insert the output into the last values
            LastValues[i] = outputs[i];
        }
        return outputs;
    }

    private double ReLU(double x)
    {
        return x > 0 ? x : 0;
    }
    private double Sigmoid(double x)
    {
        return 1 / (1 + Mathf.Exp((float)-x));
    }
}
