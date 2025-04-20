using System.Collections.Generic;
using UnityEngine;

public class CarActor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private AgentDDPG _agent;
    private CarRaycaster _radar;
    private CarControl _controller;

    public bool sessionRunning = true;

    public bool isAI = true; // Flag to indicate if the car is controlled by AI or not
    void Awake()
    {
        _radar = GetComponent<CarRaycaster>();
        _controller = GetComponent<CarControl>();
        
        if (_radar == null)
        {
            Debug.LogError("Couldn'nt get RadarScript in Actor");
        }
        if (_controller == null)
        {
            Debug.LogError("Couldn't get the Car Control script in Actor");
        }

    }
    public void Initialize(int stateSize, int[] actorHiddenLayers, int actionSize, Dictionary<string, (int, float)> hyperparameters)
    {
        _agent = new AgentDDPG(stateSize, actorHiddenLayers, actionSize, hyperparameters);

    }
    

    // Update is called once per frame
    void Update()
    {
        if (!isAI)
        {
            return;
        }
        // get the inputs from the radar
        float[] state = _radar.GetNetworkInput();
        // get the action based on the state
        float[] action = _agent.GetAction(state);

        ApplyAction(action);
        
    }





    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BadWall"))
        {
            this.sessionRunning = false;
        }
    }



    // get an array of floats that describes the action and apply it on the car
    private void ApplyAction(float[] action)
    {
        // apply the action on the motor
        _controller.Accelerate(action[0]);
        // apply the action on the wheel
        _controller.TurnWheel(action[1]);
    }

    

    // used to change the agent's weights and the amplification of the noise
    public void UpdateAgent(Actor actor, float noiseAmp)
    {
        _agent.UpdateValues(actor, noiseAmp);
    }



    // Reseting the basic values of the Agent for a rerun
    public void ResetCar()
    {
        _controller.ResetCar();
        this.sessionRunning = true;
    }
}
