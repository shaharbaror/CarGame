using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    internal class SigmoidActivation : ActivationFunction
    {
        public override float Activate(float value)
        {
            return (float)(1 / (1 + Math.Exp((double)-value)));
        }
        public override float Derivative(float value)
        {
            return value * (1 - value);
        }
    }
}
