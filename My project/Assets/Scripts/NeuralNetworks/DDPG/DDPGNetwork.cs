using UnityEngine;

public class DDPGNetwork
{
    public NeuralNet Policy;
    public NeuralNet Target;
    private float _tau;      // The rate at which the target network is updated
    protected float _learningRate = 0.001f; // Default learning rate

    public DDPGNetwork(float tau = 0.001f, float learningRate = 0)
    {
        this._tau = tau;
        _learningRate = learningRate;
    }

    public void SoftUpdateTarget()
    {
        Target.SoftUpdate(Policy, _tau);
    }

    public void CopyTargetNetwork()
    {
        // Clone the policy network to the target network
        Target.CloneNetwork(Policy);
    }

    public void SetNetworks(DDPGNetwork net)
    {
        // set the policy and target networks to be the same as the actor networks
        Policy.CloneNetwork(net.Policy);
        Target.CloneNetwork(net.Target);
    }
}
