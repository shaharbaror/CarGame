using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.WeightInits
{
    public abstract class WeightInitializer
    {
        // Generate Weight for a specific neuron
        public abstract float[] GenerateWeights(int inputSize);

        // Generate Bias for a specific neuron
        public abstract float GenerateBias();
    }
}
