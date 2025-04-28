using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CarActor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private AgentDDPG _agent;
    private CarRaycaster _radar;
    private CarControl _controller;

    private int _batchSize;

    public bool sessionRunning = true;

    public bool isAI = true; // Flag to indicate if the car is controlled by AI or not


    // values used to store experiences
    private float[] _lastState, _curState;
    private float[] _curAction;
    private float _reward;
    public bool once = true;

    private int _maxActions = 0;
    private int _numActions = 0;
    private float _coolDown = 0;
    private float _timer;

    private float _lastDist = 0;

    public AgentDDPG Agent => _agent;

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
    public void Initialize(int stateSize, int[] actorHiddenLayers, int actionSize, Dictionary<string, (int, float)> hyperparameters, int maxActions, float coolDown)
    {
        _agent = new AgentDDPG(stateSize, actorHiddenLayers, actionSize, hyperparameters);
        _batchSize = hyperparameters["batchSize"].Item1;

        _maxActions = maxActions;
        _coolDown = coolDown;
    }
    

    // Update is called once per frame
    void Update()
    {
        if (_timer < Time.time)
        {
            _timer = Time.time + _coolDown;
            if (!isAI || (!sessionRunning && !once))
            {
                return;
            }
            if (_numActions > _maxActions)
            {
                this.sessionRunning = false;
            }
            // get the inputs from the radar
            _curState = _radar.GetNetworkInput();


            this.AddExperience();
            this._reward = 0;


            // get the action based on the state
            float[] action = _agent.GetAction(_curState);

            ApplyAction(action);

            _lastState = (float[])_curState.Clone();
            _numActions++;
        }

    }

    
    private void AddExperience()
    {


        if (_lastState != null)
        {
            // caclulate reward
            CalculateReward();
            // apply experience
            _agent.AddExperience(_lastState, _curAction, _reward, _curState, !sessionRunning);

            // make sure the car will give the experience once if it hit the wall and not multiple times
            if (!sessionRunning)
            {
                once = false;
            }

        }
    }

    private float CalculateWheelReward()
    {
        if (_controller.carSpeed > 0.5f)
        {
            // calculate the distance to the wall
            float[] distances = _radar.GetSides();
            float targetRatio = 0.5f;

            //float rightDist = distances[1] / distances[0]; //   right / left

            //float ratioDiff = Mathf.Abs(rightDist - targetRatio);

            //return 2f * Mathf.Exp(-5f * ratioDiff) - 1f;
            float centeringMetric = Mathf.Min(distances[0], distances[1]) / Mathf.Max(distances[0], distances[1]);


            // Smooth reward that peaks when perfectly centered (centeringMetric = 1)
            //float amount = Mathf.Exp((centeringMetric - _lastDist)*5f);
            //_lastDist = centeringMetric;
            float amount = Mathf.Pow(centeringMetric, 2f) * 2f - 0.5f; // Squared to make it more sensitive around the center

            return Mathf.Clamp(amount, - 1f,1f);
        }
        return 0f;
        
    }

    private float CalculateMotorReward()
    {
        // calculate the speed of the car
        float speed = _controller.carSpeed;
        if (_controller.carSpeed < 0.5f)
        {
            // Progressive penalty based on how slow the car is
            return -0.3f * (0.5f - _controller.carSpeed) / 0.5f;
        }
        //if (_controller.carSpeed < _controller.prevSpeed)
        //{
        //    if (_controller.carSpeed < 1f)
        //    {
        //        return -1f;
        //    }
        //    else
        //    {
        //        return -0.3f;
        //    }
        //}

        return Mathf.Clamp(speed / _controller.maxSpeed, 0f, 1f);

    }

    private void CalculateReward()
    {
        float totalReward = 0;
        
        totalReward += CalculateWheelReward() * 0.7f; // weight of the wheel reward
        totalReward += CalculateMotorReward() * 0.3f; // weight of the motor reward

        //if (_controller.carSpeed > 3f)
        //{


        //    float steeringChange = Mathf.Abs(_controller.curSteer - _controller.prevSteer);
        //    float smoothSteeringReward = Mathf.Exp(-steeringChange * 5f); // Exponential decay for large changes
        //    totalReward += smoothSteeringReward * 0.2f; // Weight factor
        //}

        _reward += totalReward;


    }







    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BadWall"))
        {
            _reward -= 10f;
            this.sessionRunning = false;

        }
        if (collision.gameObject.CompareTag("Respawn"))
        {

            this.sessionRunning = false;
            this.once = false;
        }

    }
  



    // get an array of floats that describes the action and apply it on the car
    private void ApplyAction(float[] action)
    {
        //Debug.Log("Action: " + action[0] + ", " + action[1]);
        // apply the action on the motor
        _controller.Accelerate(action[0]);
        // apply the action on the wheel
        _controller.TurnWheel(action[1]);

        _curAction = (float[])action.Clone();
    }

    

    // used to change the agent's weights and the amplification of the noise
    public void UpdateAgent(Actor actor, float noiseAmp)
    {
        _agent.UpdateValues(actor, noiseAmp);
    }

    public ContinuousNeuralState[] GetBatch()
    {
        return _agent.GetBatch(_batchSize);
    }


    // Reseting the basic values of the Agent for a rerun
    public void ResetCar()
    {
        _controller.ResetCar();
        this.sessionRunning = true;
        this.once = true;
        this._lastState = null;
        this._curState = null;
        this._curAction = null;
        this._reward = 0;
        this._numActions = 0;
        this._timer = 0;
    }
}
