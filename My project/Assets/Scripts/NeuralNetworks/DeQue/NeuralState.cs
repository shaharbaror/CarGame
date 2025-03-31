using UnityEngine;

public class NeuralState 
{
    public float[] state, newState;
    public int action;
    public float reward;
    public bool terminated;

    public NeuralState(float[] state, int action, float reward, float[] newState, bool terminated)
    {
        this.state = state;
        this.action = action;
        this.reward = reward;
        this.newState = newState;
        this.terminated = terminated;
    }


}
