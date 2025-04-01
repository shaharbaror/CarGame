using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.Activations
{
    internal class LeakyReLUActivation : ActivationFunction
    {
        private float alpha = 0.01f;
        public override float Activate(float value)
        {
            return value > 0 ? value : value*alpha;
        }
        public override float Derivative(float value)
        {
            return value > 0 ? 1 : alpha;
        }
       

    }
}
