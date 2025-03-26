using UnityEngine;
using System;
using System.Linq;
namespace Assets.Scripts.NeuralNetworks.WeightInits
{
    
    public class XavierInitializer : WeightInitializer
    {
        public override double[] GenerateWeights(int inputSize)
        {
            double stddev = Math.Sqrt(2.0 / (inputSize + 1));
            // get the weights in float values
            return Enumerable.Range(0, inputSize)
                .Select(_ =>(double) UnityEngine.Random.Range((float)(-stddev), (float)(stddev)))
                .ToArray();
        }

        public override double GenerateBias()
        {
            return 0;
        }
    }

    
}
