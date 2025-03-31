using UnityEngine;
using System;
using System.Linq;
namespace Assets.Scripts.NeuralNetworks.WeightInits
{
    public class HeInitializer : WeightInitializer
    {
        public override float[] GenerateWeights(int inputSize)
        {
            float stddev = (float)Math.Sqrt(2.0 / inputSize);
            return Enumerable.Range(0, inputSize)
                .Select(_ => UnityEngine.Random.Range(-stddev, stddev))
                .ToArray();
           
        }

        public override float GenerateBias()
        {
            return 0;
        }
    }
}

