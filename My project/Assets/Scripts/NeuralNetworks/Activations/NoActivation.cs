using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    internal class NoActivation : ActivationFunction
    {
        public override float Activate(float value)
        {
            return value;
        }
        public override float Derivative(float value)
        {
            return 1;
        }
    }
}
