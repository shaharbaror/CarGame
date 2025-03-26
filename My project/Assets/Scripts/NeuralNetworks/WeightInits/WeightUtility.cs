using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.NeuralNetworks.WeightInits
{
    public static class WeightUtility
    {
        public static WeightInitializer GetDefaultInitializer(string activation)
        {
            return activation?.ToLower() switch
            {
                "relu" => new HeInitializer(),
                "sigmoid" or "tanh" => new XavierInitializer(),
                _ => new HeInitializer()
            };
        }
    }
}
