using UnityEngine;

// Ornstein-Uhlenbeck noise process for temporally correlated exploration
public class OUNoise
{
    private float[] _state;
    private float _mu;
    private float _theta;
    private float _sigma;
    private System.Random _random;

    public OUNoise(int size, float mu, float theta, float sigma)
    {
        // Initialize the state and the parameters
        this._state = new float[size];
        this._mu = mu;
        this._theta = theta;
        this._sigma = sigma;
        this._random = new System.Random();
        // Reset the state to the mean
        Reset();
    }

    // set the state to the mean
    public void Reset()
    {
        for (int i = 0; i < _state.Length; i++)
        {
            _state[i] = _mu;
        }
    }

    // Sample a noise vector into the state
    public float[] Sample()
    {
        for (int i =0; i< _state.Length; i++)
        {
            // Generate noise using the Ornstein-Uhlenbeck process
            float noise = _theta * (_mu - _state[i]) + _sigma * RandomGaussian();
            _state[i] += noise;
        }
        return _state;
    }

    // Generate a random number from a Gaussian distribution
    public float RandomGaussian()
    {
        // Generate a Gaussian random number using Box-Muller transform
        float u1 = 1.0f - (float)_random.NextDouble();
        float u2 = 1.0f - (float)_random.NextDouble();
        return Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
    }
}
