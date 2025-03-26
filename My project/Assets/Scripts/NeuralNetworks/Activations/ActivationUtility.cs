using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    public static class ActivationUtility
    {
        public static ActivationFunction GetActivationFunction(string name)
        {
            return name?.ToLower() switch
            {
                "relu" => new ReLUActivation(),
                "sigmoid" => new SigmoidActivation(),
                _ => new ReLUActivation() // Default
            };
        }
    }
}
