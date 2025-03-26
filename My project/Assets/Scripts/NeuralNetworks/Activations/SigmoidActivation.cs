using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    internal class SigmoidActivation : ActivationFunction
    {
        public override double Activate(double value)
        {
            return 1 / (1 + Math.Exp((float)-value));
        }
        public override double Derivative(double value)
        {
            return value * (1 - value);
        }
    }
}
