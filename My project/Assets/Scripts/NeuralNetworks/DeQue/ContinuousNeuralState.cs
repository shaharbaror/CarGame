using UnityEngine;
using System.Collections.Generic;

    // Neural state for continuous actions
public class ContinuousNeuralState : NeuralState
{
    public new float[] action; // Override the int action with float array for continuous actions

    public ContinuousNeuralState(float[] state, float[] action, float reward, float[] newState, bool terminated)
        : base(state, 0, reward, newState, terminated) // Pass 0 as dummy action to base constructor
    {
        this.action = action;
    }
}


