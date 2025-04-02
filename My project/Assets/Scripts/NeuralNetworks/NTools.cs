using System;
using Unity.VisualScripting;
using UnityEngine;

public class NTools
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LayerData SerializeLayer(Layer l)
    {
        LayerData data = new LayerData();
        data.InputSize = l.InputSize;
        data.NeuronCount = l.NeuronCount;
        data.Biases = l.Biases;
        data.Weights = l.Weights;
        return data;
    }

    public NetworkData SerializeNetwork(NeuralNet net) {
        NetworkData n = new NetworkData();
        n.LayerCount = net.GetLayerCount();
        n.Layers = new LayerData[n.LayerCount];
        for (int i =0; i< n.LayerCount; i++)
        {
            n.Layers[i] = this.SerializeLayer(net.GetLayerAtIndex(i));
        }
        return n;
    }

    public void DeserializeLayer(Layer l, LayerData data)
    {
        l.SetLayer(data.Weights, data.Biases);
        l.InputSize = data.InputSize;
        l.NeuronCount = data.NeuronCount;
    }

    public void DeserializeNetwork(NeuralNet net, NetworkData data)
    {
        for (int i = 0; i < data.LayerCount; i++)
        {
            this.DeserializeLayer(net.GetLayerAtIndex(i), data.Layers[i]);
        }
    }
}

[Serializable]
public class NetworkData
{
    public int LayerCount;
    public LayerData[] Layers;
}

[Serializable]
public class LayerData
{
    public int InputSize;
    public int NeuronCount;
    public float[] Biases;
    public float[,] Weights;
}
