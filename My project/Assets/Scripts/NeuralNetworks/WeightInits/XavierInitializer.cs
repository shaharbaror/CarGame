using UnityEngine;
using System;
using System.Linq;
namespace Assets.Scripts.NeuralNetworks.WeightInits
{
    
    public class XavierInitializer : WeightInitializer
    {
        private int currentLayerSize;

        public void setLayerSize(int layerSize)
        {
            currentLayerSize = layerSize;
        }
        
        public override double[] GenerateWeights(int inputSize)
        {


            double stddev = Math.Sqrt(2.0 / (inputSize + currentLayerSize));
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
