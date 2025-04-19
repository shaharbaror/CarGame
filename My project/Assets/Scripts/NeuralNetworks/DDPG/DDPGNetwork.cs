using UnityEngine;

public class DDPGNetwork
{
    public NeuralNet Policy;
    public NeuralNet Target;
    private float _tau;      // The rate at which the target network is updated

    public DDPGNetwork(float tau = 0.001f)
    {
        this._tau = tau;
    }

    public void SoftUpdateTarget()
    {
        Target.SoftUpdate(Policy, _tau);
    }

    public void CopyNetworks()
    {
        // Clone the policy network to the target network
        Target.CloneNetwork(Policy);
    }
}
