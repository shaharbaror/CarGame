using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    internal class ReLUActivation : ActivationFunction
    {
        public override double Activate(double value)
        {
            return value > 0 ? value : 0;
        }
        public override double Derivative(double value)
        {
            return value > 0 ? 1 : 0;
        }
    }
}
