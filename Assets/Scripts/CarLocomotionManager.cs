using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum GearState
{
	Neutral,
	Reversing,
	Running,
	CheckingChange,
	ChangingGear
}

public class CarLocomotionManager : MonoBehaviour
{
	#region Variables

	#region General Settings

	public Rigidbody PlayerCarRb { get; private set; }
	public Transform PlayerCar { get; private set; }

	[HideInInspector] public bool isReversing;

	#endregion

	#region Motor Settings

	[Header("Motor Settings")]
	[SerializeField] private float motorPower;
	public float maxSpeed;
	private float clampedSpeed;

	[HideInInspector] public float carSpeedRatio;
	[HideInInspector] public int engineStatus;

	#endregion

	#region Steering Settings

	[Header("Steering Settings")]
	[SerializeField] private float wheelBase;
	[SerializeField] private float rearTrack;
	[SerializeField] private float turnRadius;
	private float ackermannAngleLeft;
	private float ackermannAngleRight;

	#endregion

	#region Break Settings

	[Header("Break Settings")]
	[SerializeField] private float brakePower;
	// [SerializeField] private Material brakeMaterial;
	// [SerializeField] private Color brakingColor;
	// [SerializeField] private float brakeColorIntensity;
	[HideInInspector] public float brakeInput;
	// private bool handBrake;

	#endregion

	#region Gears & RPM Settings

	[Header("Gears & RPM")]
	public float redLine;
	[SerializeField] private float idleRpm;
	[SerializeField] private float minNeedleRotation;
	[SerializeField] private float maxNeedleRotation;
	[SerializeField] private float reverseGearRatio = -2.0f;
	[SerializeField] private float[] gearRatios;
	[SerializeField] private float differentialRatio;
	[SerializeField] private AnimationCurve powerCurve;
	[SerializeField] private float increaseGearRpm;
	[SerializeField] private float decreaseGearRpm;
	[SerializeField] private float changeGearDelay = 0.5f;
	[HideInInspector] public float currentRpm;
	private int currentGear;
	private float currentTorque;
	private float clutch;
	private float wheelRpm;
	private GearState gearState;

	#endregion

	#region UI Settings

	[Header("UI Settings")]
	[SerializeField] private TMP_Text speedText;
	[SerializeField] private TMP_Text rpmText;
	[SerializeField] private TMP_Text gearText;
	[SerializeField] private Transform rpmNeedle;

	#endregion

	#region Wheel Settings

	[Header("Wheels Settings")]
	public WheelColliders wheelColliders;
	[SerializeField] private WheelMeshRenderers wheelMeshRenderers;
	public WheelFrictionCurve forwardFrictionCurve;
	public WheelFrictionCurve sidewaysFrictionCurve;
	[HideInInspector] public float forwardFrictionVelocity;
	[HideInInspector] public float sidewaysFrictionVelocity;

	#endregion

	#region Drift Settings

	// [Header("Drift Settigns")]
	// [SerializeField] private float driftForce = 500f; // Force to apply to the rear end to initiate drift
	// [SerializeField] private float driftGraceDuration = 2.0f; // Duration to keep drifting state active after handbrake is released
	// private float driftTimer = 0f;
	// public event EventHandler IsDriftingEvent;
	// public event EventHandler IsNotDriftingEvent;

	#endregion

	#endregion

	private void Awake()
	{
		PlayerCar = this.transform;
		PlayerCarRb = PlayerCar.GetComponent<Rigidbody>();
	}

	private void Start()
	{
		// powerCurve = new AnimationCurve(
		// 		new Keyframe(idleRpm - 200 / redLine, 0 / motorPower),        // slightly below idle, power is 0
		// 		new Keyframe(3000 / redLine, 50 / motorPower),       // Low power at 3,000 RPM
		// 		new Keyframe(5000 / redLine, 200 / motorPower),      // Increasing power at 5,000 RPM
		// 		new Keyframe(7500 / redLine, 500 / motorPower),      // Significant power increase at 7,500 RPM
		// 		new Keyframe(10000 / redLine, 800 / motorPower),     // Power climbing at 10,000 RPM
		// 		new Keyframe(12500 / redLine, 950 / motorPower),     // Close to peak power at 12,500 RPM
		// 		new Keyframe(redLine / redLine, 900 / motorPower)      // Slight drop-off at redline (15,000 RPM)
		// 	);

		// reversePowerCurve = new AnimationCurve(
		// 	new Keyframe(0, 0),               // At 0 RPM, power is 0
		// 	new Keyframe(1000 / redLine, 50 / motorPower),    // Low power at 1000 RPM
		// 	new Keyframe(3000 / redLine, 150 / motorPower),   // Moderate power at 3000 RPM
		// 	new Keyframe(5000 / redLine, 200 / motorPower)    // Peak power at 5000 RPM
		// );

		gearState = GearState.Neutral;
	}

	private void Update()
	{
		clampedSpeed = Mathf.Lerp(clampedSpeed, GetCarSpeed(), Time.deltaTime);
	}

	public void HandleCarLocomotion(float throttleInput, float steerInput, float clutchInput, float handBrakeInput)
	{
		HandleStartingEngine(throttleInput);
		HandleMotor(throttleInput);
		HandleSteering(steerInput);
		HandleWheels();
		HandleBrake(handBrakeInput);
		HandleBrakeDuringSlip(throttleInput);
		UpdateGaugeClusterUI();
		HandleClutch(throttleInput, clutchInput);
		// HandleDrift();
		UpdateReversing(throttleInput);
		UpdateCarSpeedRatio(throttleInput);


		if (gearState == GearState.Reversing && throttleInput >= 0)
		{
			gearState = GearState.Neutral;
		}
	}

	private void HandleStartingEngine(float throttleInput)
	{
		if (Mathf.Abs(throttleInput) > 0 && engineStatus == 0)
		{
			StartCoroutine(GetComponent<EngineAudioManager>().StartEngine());
			gearState = GearState.Running;
		}
	}

	private void HandleMotor(float throttleInput)
	{
		float carSpeed = GetCarSpeed();

		if (carSpeed > maxSpeed)
		{
			wheelColliders.rearRightWheel.motorTorque = 0;
			wheelColliders.rearLeftWheel.motorTorque = 0;
		}
		else
		{
			if (Mathf.Abs(throttleInput) > 0 && changeGearDelay >= 0.7f)
			{
				gearState = GearState.Running;
				changeGearDelay -= Time.deltaTime;
				if (changeGearDelay <= 0)
				{
					changeGearDelay = 0.7f;
				}
			}

			currentTorque = CalculateTorque(throttleInput);
			float motorTorque = currentTorque * throttleInput;

			if (isReversing)
			{
				if (GetCarSpeed() * 3.6 > 52)
				{
					motorTorque = 0;
				}
			}

			wheelColliders.rearRightWheel.motorTorque = motorTorque;
			wheelColliders.rearLeftWheel.motorTorque = motorTorque;
		}
	}

	private float CalculateTorque(float throttleInput)
	{
		float torque = 0;

		if (currentRpm < idleRpm + 200 && throttleInput == 0 && currentGear == 0)
		{
			gearState = GearState.Neutral;
			UpdateGaugeClusterUI();
		}

		if (gearState == GearState.Running && clutch > 0)
		{
			if (currentRpm > increaseGearRpm)
			{
				StartCoroutine(ChangeGear(1, throttleInput));
			}
			else if (currentRpm < decreaseGearRpm)
			{
				if (currentGear > 0)
				{
					StartCoroutine(ChangeGear(-1, throttleInput));
				}
				else if (currentGear == 0)
				{
					if (throttleInput < 0)
					{
						StartCoroutine(ChangeGear(-1, throttleInput));
					}
				}
			}
		}

		if (engineStatus > 0)
		{
			if (clutch < 0.1f)
			{
				Debug.Log("Engine Stalled");
				currentRpm = Mathf.Lerp(currentRpm, Mathf.Max(idleRpm, redLine * Mathf.Abs(throttleInput)) + UnityEngine.Random.Range(-50, 50), Time.deltaTime);
			}
			else
			{
				Debug.Log("Engine Running");
				wheelRpm = Mathf.Abs((wheelColliders.rearRightWheel.rpm + wheelColliders.rearLeftWheel.rpm) / 2f) * (isReversing ? reverseGearRatio : gearRatios[currentGear]) * differentialRatio;
				currentRpm = Mathf.Lerp(currentRpm, Mathf.Max(idleRpm - 100, wheelRpm), Time.deltaTime * 3f);

				if (isReversing)
				{
					Debug.Log("Reversing");
					torque = powerCurve.Evaluate(currentRpm / redLine) * motorPower / currentRpm * reverseGearRatio * differentialRatio * 5252f * clutch;
				}
				else
				{
					Debug.Log("Forward");
					torque = powerCurve.Evaluate(currentRpm / redLine) * motorPower / currentRpm * gearRatios[currentGear] * differentialRatio * 5252f * clutch;
				}
			}
		}

		return torque;
	}

	private void HandleSteering(float steerInput)
	{
		// Check if there's any steering input
		if (steerInput > 0)
		{
			ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
			ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
		}
		else if (steerInput < 0)
		{
			ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
			ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
		}
		else
		{
			ackermannAngleLeft = 0;
			ackermannAngleRight = 0;
		}

		// Apply the calculated steering angles
		wheelColliders.frontLeftWheel.steerAngle = ackermannAngleLeft;
		wheelColliders.frontRightWheel.steerAngle = ackermannAngleRight;
	}

	private void HandleWheels()
	{
		UpdateWheelPosition(wheelColliders.frontLeftWheel, wheelMeshRenderers.frontLeftWheel);
		UpdateWheelPosition(wheelColliders.frontRightWheel, wheelMeshRenderers.frontRightWheel);
		UpdateWheelPosition(wheelColliders.rearLeftWheel, wheelMeshRenderers.rearLeftWheel);
		UpdateWheelPosition(wheelColliders.rearRightWheel, wheelMeshRenderers.rearRightWheel);
	}

	private void UpdateWheelPosition(WheelCollider wheelCollider, MeshRenderer wheelMeshRenderer)
	{
		Vector3 position;
		Quaternion rotation;
		wheelCollider.GetWorldPose(out position, out rotation);
		wheelMeshRenderer.transform.position = position;
		wheelMeshRenderer.transform.rotation = rotation;
	}

	private void HandleBrake(float handBrakeInput)
	{
		float frontWheelBrakeTorque = brakeInput * brakePower * 0.7f;

		wheelColliders.frontLeftWheel.brakeTorque = frontWheelBrakeTorque;
		wheelColliders.frontRightWheel.brakeTorque = frontWheelBrakeTorque;

		float rearWheelBrakeTorque = brakeInput * brakePower * 0.3f;

		wheelColliders.rearLeftWheel.brakeTorque = rearWheelBrakeTorque;
		wheelColliders.rearRightWheel.brakeTorque = rearWheelBrakeTorque;

		// if (brakeMaterial)
		// {
		// 	if (brakeInput > 0)
		// 	{
		// 		brakeMaterial.EnableKeyword("_EMISSION");
		// 		brakeMaterial.SetColor("_EmissionColor", brakingColor * Mathf.Pow(2, brakeColorIntensity));
		// 	}
		// 	else
		// 	{
		// 		brakeMaterial.DisableKeyword("_EMISSION");
		// 		brakeMaterial.SetColor("_EmissionColor", Color.black);
		// 	}
		// }

		// handBrake = handBrakeInput > 0.5;

		// if (handBrake)
		// {
		// 	clutch = 0;
		// 	wheelColliders.rearRightWheel.brakeTorque = brakePower * 1000f;
		// 	wheelColliders.rearLeftWheel.brakeTorque = brakePower * 1000f;
		// }
	}

	private void HandleBrakeDuringSlip(float throttleInput)
	{
		float movingDirection = Vector3.Dot(transform.forward, PlayerCarRb.linearVelocity);

		if (movingDirection < -0.5f && throttleInput > 0)
		{
			brakeInput = Mathf.Abs(throttleInput);
		}
		else if (movingDirection > 0.5f && throttleInput < 0)
		{
			brakeInput = Mathf.Abs(throttleInput);
		}
		else
		{
			brakeInput = 0;
		}
	}

	private void UpdateGaugeClusterUI()
	{
		if (rpmNeedle)
		{
			rpmNeedle.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minNeedleRotation, maxNeedleRotation, currentRpm / (redLine * 1.1f)));
		}

		if (rpmText)
		{
			rpmText.text = currentRpm.ToString("0000") + " RPM";
		}

		if (gearText)
		{
			gearText.text = (gearState == GearState.Neutral) ? "N"
						: isReversing ? "R"
						: (currentGear + 1).ToString();
		}

		if (speedText)
		{
			float carSpeed = GetCarSpeed() * 3.6f;
			speedText.text = carSpeed.ToString("0") + " km/h";
		}
	}

	private void HandleClutch(float throttleInput, float clutchInput)
	{
		if (gearState != GearState.ChangingGear)
		{
			if (gearState == GearState.Neutral)
			{
				clutch = 0;
				if (Mathf.Abs(throttleInput) > 0) gearState = GearState.Running;
			}
			else
			{
				clutch = Mathf.Lerp(clutch, Mathf.Abs(1 - clutchInput), Time.deltaTime * 5f);
			}
		}
		else
		{
			clutch = Mathf.Lerp(clutch, 0, Time.deltaTime * 5f);
		}
	}

	// private void HandleDrift()
	// {
	// 	float carSpeed = GetCarSpeed() * 3.6f; // Convert to km/h

	// 	// Check if the car should start drifting
	// 	if (!isDrifting && carSpeed > 40f && Mathf.Abs(dampenedSteeringInput) > 0.1f && handBrake)
	// 	{
	// 		StartDrifting();
	// 	}

	// 	// If the car is drifting, keep isDrifting true as long as the car is not straight
	// 	if (isDrifting)
	// 	{
	// 		driftTimer += Time.deltaTime;

	// 		float angleBetween = Vector3.Angle(transform.forward, playerCarRb.linearVelocity);

	// 		if (angleBetween < 5f)
	// 		{
	// 			if (driftTimer > driftGraceDuration)
	// 			{
	// 				ResetDriftingState();
	// 			}
	// 		}
	// 		else
	// 		{
	// 			driftTimer = 0; // Reset the timer if the car is not moving straight
	// 		}
	// 	}
	// }

	// private void StartDrifting()
	// {
	// 	Debug.Log("Drifting");

	// 	IsDriftingEvent?.Invoke(this, EventArgs.Empty);
	// 	isDrifting = true;

	// 	driftTimer = 0;

	// 	Vector3 rearPosition = transform.position - transform.forward * 2.0f; // Adjust the offset as needed
	// 	Vector3 driftDirection = Vector3.Cross(playerCarRb.linearVelocity.normalized, Vector3.up); // Determine the direction of the drift
	// 	driftDirection *= dampenedSteeringInput > 0 ? 1 : -1; // Adjust direction based on steering input
	// 	playerCarRb.AddForceAtPosition(driftDirection * driftForce, rearPosition, ForceMode.Impulse);

	// 	// slightly apply brake torque
	// 	wheelColliders.rearRightWheel.brakeTorque = brakePower * 0.25f;
	// 	wheelColliders.rearLeftWheel.brakeTorque = brakePower * 0.25f;
	// }

	// private void ResetDriftingState()
	// {
	// 	Debug.Log("Not Drifting");

	// 	IsNotDriftingEvent?.Invoke(this, EventArgs.Empty);
	// 	isDrifting = false;
	// 	driftTimer = 0;

	// 	// Reset the brake torque
	// 	wheelColliders.rearRightWheel.brakeTorque = 0;
	// 	wheelColliders.rearLeftWheel.brakeTorque = 0;
	// }

	// public float GetDriftAngle()
	// {
	// 	// Project velocity onto the horizontal plane
	// 	Vector3 velocityOnPlane = Vector3.ProjectOnPlane(playerCarRb.linearVelocity, Vector3.up);
	// 	Vector3 forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

	// 	// Calculate the angle between the velocity and the forward direction
	// 	float angle = Vector3.Angle(forwardOnPlane, velocityOnPlane);

	// 	// Determine the sign of the angle based on the cross product
	// 	float sign = Mathf.Sign(Vector3.Cross(forwardOnPlane, velocityOnPlane).y);

	// 	// Set the current drift angle with the correct sign
	// 	return angle * sign;
	// }

	public float GetCarSpeed()
	{
		return PlayerCarRb.linearVelocity.magnitude;
	}

	public void UpdateCarSpeedRatio(float throttleInput)
	{
		float throttle = Mathf.Clamp(Mathf.Abs(throttleInput), 0.5f, 1f);

		carSpeedRatio = currentRpm * throttle / redLine;
	}

	private void UpdateReversing(float throttleInput)
	{
		if (gearState == GearState.Reversing)
		{
			isReversing = throttleInput < 0;
		}
		else
		{
			isReversing = false;
		}
	}

	private IEnumerator ChangeGear(int gearChange, float throttleInput)
	{
		gearState = GearState.CheckingChange;

		if (currentGear + gearChange >= -1f)
		{
			if (gearChange > 0)
			{
				yield return new WaitForSeconds(0.7f);

				if (currentRpm < increaseGearRpm || currentGear >= gearRatios.Length - 1)
				{
					gearState = GearState.Running;
					yield break;
				}
			}

			if (gearChange < 0)
			{
				yield return new WaitForSeconds(0.1f);

				if (currentGear == 0 && throttleInput < 0)
				{
					gearState = GearState.Reversing;
					yield break;
				}
				else if (currentRpm > decreaseGearRpm || currentGear <= -1)
				{
					gearState = GearState.Running;
					yield break;
				}
			}

			gearState = GearState.ChangingGear;
			yield return new WaitForSeconds(changeGearDelay);
			currentGear += gearChange;
		}

		if (gearState != GearState.Neutral)
		{
			gearState = GearState.Running;
		}
	}
}

[Serializable]
public class WheelColliders
{
	public WheelCollider frontLeftWheel, frontRightWheel;
	public WheelCollider rearLeftWheel, rearRightWheel;
}

[Serializable]
public class WheelMeshRenderers
{
	public MeshRenderer frontLeftWheel, frontRightWheel;
	public MeshRenderer rearLeftWheel, rearRightWheel;
}