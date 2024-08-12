using UnityEngine;

public class VehicleStabiliser : MonoBehaviour
{
	[SerializeField] private float AntiRoll = 5000.0f;

	private CarLocomotionManager carLocomotionManager;
	private WheelCollider rearLeftWheel;
	private WheelCollider rearRightWheel;

	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();

		rearLeftWheel = carLocomotionManager.wheelColliders.rearLeftWheel;
		rearRightWheel = carLocomotionManager.wheelColliders.rearRightWheel;
	}

	private void FixedUpdate()
	{
		WheelHit hit;

		float travelL = 1.0f;
		float travelR = 1.0f;

		bool groundedL = rearLeftWheel.GetGroundHit(out hit);

		if (groundedL)
		{
			travelL = (-rearLeftWheel.transform.InverseTransformPoint(hit.point).y - rearLeftWheel.radius) / rearLeftWheel.suspensionDistance;
		}

		bool groundedR = rearRightWheel.GetGroundHit(out hit);

		if (groundedR)
		{
			travelR = (-rearRightWheel.transform.InverseTransformPoint(hit.point).y - rearRightWheel.radius) / rearRightWheel.suspensionDistance;
		}

		float antiRollForce = (travelL - travelR) * AntiRoll;

		if (groundedL)
		{
			carLocomotionManager.PlayerCarRb.AddForceAtPosition(rearLeftWheel.transform.up * -antiRollForce, rearLeftWheel.transform.position);
		}

		if (groundedR)
		{
			carLocomotionManager.PlayerCarRb.AddForceAtPosition(rearRightWheel.transform.up * antiRollForce, rearRightWheel.transform.position);
		}
	}
}
