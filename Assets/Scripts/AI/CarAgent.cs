using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{
	private CarLocomotionManager carLM;

	private void Awake()
	{
		carLM = GetComponent<CarLocomotionManager>();
	}

	private void Start()
	{
		RaceManager.Instance.OnCorrectCheckpoint += TrackCheckpoints_OnCorrectCheckpoint;
		RaceManager.Instance.OnWrongCheckpoint += TrackCheckpoints_OnWrongCheckpoint;
	}

	private void TrackCheckpoints_OnCorrectCheckpoint(object sender, Transform e)
	{
		if (e == transform)
		{
			Debug.Log("AI rewarded for correct checkpoint");
			AddReward(1f);
		}
	}

	private void TrackCheckpoints_OnWrongCheckpoint(object sender, RaceManager.PenaltyEventArgs e)
	{
		if (e.carTransform == transform)
		{
			Debug.Log("AI penalized for wrong checkpoint");
			AddReward(-1f);
		}
	}

	public override void OnEpisodeBegin()
	{
		if (RaceManager.Instance.IsTrainingAI)
		{
			RaceManager.Instance.StartNewEpisode(transform);
		}
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		int nextCheckpointIndex = RaceManager.Instance.nextCheckpointIndexDict[transform];
		Vector3 nextCheckpointForward = RaceManager.Instance.checkpointList[nextCheckpointIndex].transform.forward;
		float directionDot = Vector3.Dot(transform.forward, nextCheckpointForward);
		sensor.AddObservation(directionDot);
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		float throttle = 0;
		float steering = 0;
		float clutch = 0;

		switch (actions.DiscreteActions[0])
		{
			case 0:
				throttle = CarInputManager.DampenedInput(throttle, 0.1f); break;
			case 1:
				throttle = CarInputManager.DampenedInput(throttle, 1f); break;
			case 2:
				throttle = CarInputManager.DampenedInput(throttle, -1f); break;
		}

		switch (actions.DiscreteActions[1])
		{
			case 0:
				steering = CarInputManager.DampenedInput(steering, 0.1f); break;
			case 1:
				steering = CarInputManager.DampenedInput(steering, 1f); break;
			case 2:
				steering = CarInputManager.DampenedInput(steering, -1f); break;
		}

		switch (actions.DiscreteActions[2])
		{
			case 0:
				clutch = CarInputManager.DampenedInput(clutch, 0.1f); break;
			case 1:
				clutch = CarInputManager.DampenedInput(clutch, 1f); break;
		}

		carLM.HandleCarLocomotion(throttle, steering, clutch);
	}

	public override void Heuristic(in ActionBuffers actionsOut)
	{
		int throttleAction = 0;
		if (Input.GetKeyDown(KeyCode.W)) throttleAction = 1;
		if (Input.GetKeyDown(KeyCode.LeftShift)) throttleAction = 2;

		int steeringAction = 0;
		if (Input.GetKeyDown(KeyCode.D)) steeringAction = 1;
		if (Input.GetKeyDown(KeyCode.A)) steeringAction = 2;

		int clutchAction = 0;
		if (Input.GetKeyUp(KeyCode.Q)) clutchAction = 0;
		if (Input.GetKeyDown(KeyCode.Q)) clutchAction = 1;

		ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
		discreteActions[0] = throttleAction;
		discreteActions[1] = steeringAction;
		discreteActions[2] = clutchAction;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Wall"))
		{
			Debug.Log("AI penalized for collision");
			AddReward(-0.5f);
		}

		if (collision.gameObject.CompareTag("Curb"))
		{
			Debug.Log("AI rewarded for following curbs");
			AddReward(0.05f);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.CompareTag("Wall"))
		{
			Debug.Log("AI penalized for collision");
			AddReward(-0.1f);
		}

		if (collision.gameObject.CompareTag("Curb"))
		{
			Debug.Log("AI rewarded for following curbs");
			AddReward(0.005f);
		}
	}
}
