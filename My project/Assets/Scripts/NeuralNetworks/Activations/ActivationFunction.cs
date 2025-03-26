using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    public abstract class ActivationFunction
    {
        // Forward activation method
        public abstract double Activate(double value);

        // Derivative of the activation function
        public abstract double Derivative(double value);
    }
}
