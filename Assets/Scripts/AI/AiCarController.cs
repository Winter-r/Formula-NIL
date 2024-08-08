using System.Collections.Generic;
using UnityEngine;
using PathCreation;

[RequireComponent(typeof(CarLocomotionManager))]
public class AiCarController : MonoBehaviour
{
	[SerializeField] private PathCreator raceLine;
	[SerializeField] private float lookAheadDistance = 5f;
	[SerializeField] private float safeDistance = 2f;

	private CarLocomotionManager carLocomotionManager;
	private float maxAngle = 45f;
	private float dampenedThrottleInput;

	[HideInInspector] public bool shouldBrake;

	private float previousAngle;
	private float integral;
	private float kp = 0.1f; // Proportional gain
	private float ki = 0.01f; // Integral gain
	private float kd = 0.1f; // Derivative gain

	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();
	}

	private void Update()
	{
		FollowRaceLine();
	}

	private void FollowRaceLine()
	{
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 targetPoint = raceLine.path.GetPointAtDistance(raceLine.path.GetClosestDistanceAlongPath(transform.position) + lookAheadDistance);
		Vector3 direction = targetPoint - transform.position;
		float angle = Vector3.SignedAngle(forward, direction, Vector3.up);
		float closestDistance = Vector3.Distance(transform.position, targetPoint);
		float throttleInput = Mathf.Clamp01(1f - Mathf.Abs(carLocomotionManager.GetCarSpeed() * 0.02f * angle) / maxAngle);

		// PID controller for steering
		float derivative = (angle - previousAngle) / Time.deltaTime;
		integral += angle * Time.deltaTime;
		float pidSteering = kp * angle + ki * integral + kd * derivative;

		if (closestDistance < safeDistance && carLocomotionManager.GetCarSpeed() > 1f)
		{
			shouldBrake = true;
		}
		else
		{
			shouldBrake = false;
		}

		if (shouldBrake)
		{
			throttleInput = -1f;
		}

		Debug.Log("Throttle: " + throttleInput + " Steering: " + pidSteering / maxAngle);

		carLocomotionManager.HandleCarLocomotion(throttleInput, pidSteering / maxAngle, 0, 0);

		previousAngle = angle;

		Debug.DrawRay(transform.position, direction, Color.red);
	}
}
