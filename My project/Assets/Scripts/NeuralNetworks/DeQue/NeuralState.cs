using UnityEngine;

public class NeuralState 
{
    public double[] state, newState;
    public int action;
    public double reward;
    public bool terminated;

    public NeuralState(double[] state, int action, double reward, double[] newState, bool terminated)
    {
        this.state = state;
        this.action = action;
        this.reward = reward;
        this.newState = newState;
        this.terminated = terminated;
    }


}
