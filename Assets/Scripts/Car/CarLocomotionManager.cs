using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum GearState
{
	Neutral,
	Reversing,
	Running,
	CheckingChange,
	ChangingGear
}

public enum ViewType
{
	Regular,
	Far,
	Pod
}

[Serializable]
public class CameraView
{
	public ViewType viewType;
	public Transform viewTransform;
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

public class CarLocomotionManager : MonoBehaviour
{
	#region Variables

	#region General Settings

	public Rigidbody PlayerCarRb { get; private set; }
	public Transform PlayerCar { get; private set; }

	public bool isInputEnabled = true;

	[HideInInspector] public bool isReversing;

	public event Action<int> OnGearChanged;

	#endregion

	#region Input Settings

	private float throttleInput = 0f;
	private float steerInput = 0f;
	private float clutchInput = 0f;

	#endregion

	#region Motor Settings

	[Header("Motor Settings")]
	[SerializeField, Tooltip("Unit is HP")] private float motorPower;
	[Tooltip("Unit is m/s")] public float maxSpeed;
	private float clampedSpeed;

	private const float TORQUE_MULTIPLIER = 5252f;

	[HideInInspector] public float carSpeedRatio;
	[HideInInspector] public int engineStatus;

	#endregion

	#region Steering Settings

	[Header("Steering Settings")]
	[SerializeField] private float wheelBase;
	[SerializeField] private float rearTrack;
	[SerializeField] private float turnRadius;
	[SerializeField] private float maxSteeringAngle;

	#endregion

	#region Break Settings

	[Header("Break Settings")]
	[SerializeField] private float brakePower;
	[HideInInspector] public float brakeInput;

	#endregion

	#region Gears & RPM Settings

	[Header("Gears & RPM")]
	public float redLine;
	public float idleRpm;
	[SerializeField] private float minNeedleRotation;
	[SerializeField] private float maxNeedleRotation;
	[SerializeField] private float reverseGearRatio = -2.0f;
	[SerializeField] private float[] gearRatios;
	[SerializeField] private float differentialRatio;
	[SerializeField] private AnimationCurve powerCurve;
	[SerializeField] private float increaseGearRpm;
	[SerializeField] private float decreaseGearRpm;
	[SerializeField] private float changeGearDelay = 0.5f;
	[SerializeField] private float rpmSpeedFactor = 2.5f;
	[HideInInspector] public float currentRpm;
	private int currentGear;
	private float currentTorque;
	private float clutch;
	private float wheelRpm;
	private GearState gearState;

	#endregion

	#region UI Settings

	// Gauge Cluster UI
	private TMP_Text speedText;
	private TMP_Text rpmText;
	private TMP_Text gearText;
	private Transform rpmNeedle;

	#endregion

	#region Camera Settings

	[Header("Camera Settings")]
	public Transform cameraLookAt;
	public Transform podCameraLookAt;
	public CameraView[] cameraViews;

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

	#endregion

	private void Awake()
	{
		PlayerCar = this.transform;
		PlayerCarRb = PlayerCar.GetComponent<Rigidbody>();
	}

	private void Start()
	{
		gearState = GearState.Neutral;
	}

	private void Update()
	{
		FindUIElements();

		clampedSpeed = Mathf.Lerp(clampedSpeed, GetCarSpeed(), Time.deltaTime);
	}

	public void HandleCarLocomotion(float throttleInput, float steerInput, float clutchInput)
	{
		this.throttleInput = throttleInput;
		this.steerInput = steerInput;
		this.clutchInput = clutchInput;

		HandleEngine();
		HandleSteering();
		HandleBrake();
		HandleBrakeDuringSlip();
		HandleStartingEngine();
		HandleClutch();
		HandleWheels();
		UpdateReversing();
		UpdateCarSpeedRatio();
		UpdateGaugeClusterUI();

		if (gearState == GearState.Reversing && throttleInput >= 0)
		{
			gearState = GearState.Neutral;
		}
	}

	private void HandleStartingEngine()
	{
		if (Mathf.Abs(throttleInput) > 0 && engineStatus == 0)
		{
			StartCoroutine(GetComponent<EngineAudioManager>().StartEngine());
			gearState = GearState.Running;
		}
	}

	private void HandleEngine()
	{
		if (RaceManager.Instance.raceType == RaceType.TimeTrial && !RaceManager.Instance.TrialStarted)
		{
			// If race hasn't started, skip applying motor torque but allow RPM to increase with clutch
			currentTorque = CalculateTorque();
			return;
		}

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

			currentTorque = CalculateTorque();
			float motorTorque = currentTorque * throttleInput;

			wheelColliders.rearRightWheel.motorTorque = motorTorque;
			wheelColliders.rearLeftWheel.motorTorque = motorTorque;
		}
	}

	private float CalculateTorque()
	{
		float torque = 0;

		float carSpeedKmh = GetCarSpeed() * 3.6f;

		if (currentRpm < idleRpm + 200 && throttleInput == 0 && currentGear == 0)
		{
			gearState = GearState.Neutral;
			UpdateGaugeClusterUI();
		}

		float[] gearSpeedLimits = { 0f, 90f, 120f, 150f, 190f, 220f, 250f, 285f, 350f };

		if (gearState == GearState.Running && clutch > 0 && RaceManager.Instance.TrialStarted)
		{
			if (currentGear < gearSpeedLimits.Length - 1 && carSpeedKmh >= gearSpeedLimits[currentGear] && carSpeedKmh < gearSpeedLimits[currentGear + 1])
			{
				StartCoroutine(ChangeGear(1, throttleInput));
			}
			else if (currentGear > 0 && currentRpm < decreaseGearRpm)
			{
				StartCoroutine(ChangeGear(-1, throttleInput));
			}
		}

		if (engineStatus > 0)
		{
			clutch = Mathf.Clamp(clutch, 0f, 1f);

			if (clutch < 0.1f)
			{
				currentRpm = Mathf.Lerp(currentRpm, Mathf.Max(idleRpm, redLine * throttleInput) + UnityEngine.Random.Range(-50, 50), Time.deltaTime);
			}
			else
			{
				wheelRpm = Mathf.Abs((wheelColliders.rearRightWheel.rpm + wheelColliders.rearLeftWheel.rpm) / 2f) * (isReversing ? reverseGearRatio : gearRatios[currentGear]) * differentialRatio;
				currentRpm = Mathf.Lerp(currentRpm, Mathf.Max(idleRpm - 100, wheelRpm), Time.deltaTime * rpmSpeedFactor);

				if (currentRpm > 0)
				{
					if (isReversing)
					{
						torque = powerCurve.Evaluate(currentRpm / redLine) * motorPower / currentRpm * reverseGearRatio * differentialRatio * TORQUE_MULTIPLIER * clutch;
					}
					else
					{
						torque = powerCurve.Evaluate(currentRpm / redLine) * motorPower / currentRpm * gearRatios[currentGear] * differentialRatio * TORQUE_MULTIPLIER * clutch;
					}
				}
				else
				{
					torque = 0;
				}
			}
		}

		return torque;
	}

	private void HandleSteering()
	{
		float carSpeed = GetCarSpeed();
		float speedFactor = Mathf.Clamp01(carSpeed / maxSpeed);

		// Adjust steering input based on speed
		float adjustedSteerInput = steerInput * Mathf.Lerp(1.0f, 0.3f, speedFactor);
		float dynamicMaxSteeringAngle = Mathf.Lerp(maxSteeringAngle, maxSteeringAngle * 0.5f, speedFactor);

		// Calculate Ackermann steering angles
		float innerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * adjustedSteerInput;
		float outerAngle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * adjustedSteerInput;

		// Interpolate steering angles for low-speed smoothness
		if (carSpeed < 5f)
		{
			float lowSpeedSteer = Mathf.Lerp(innerAngle, steerInput * maxSteeringAngle, 1f - (carSpeed / 5f));
			wheelColliders.frontLeftWheel.steerAngle = steerInput > 0 ? lowSpeedSteer : lowSpeedSteer;
			wheelColliders.frontRightWheel.steerAngle = steerInput > 0 ? lowSpeedSteer : lowSpeedSteer;
		}
		else
		{
			wheelColliders.frontLeftWheel.steerAngle = steerInput > 0 ? innerAngle : outerAngle;
			wheelColliders.frontRightWheel.steerAngle = steerInput > 0 ? outerAngle : innerAngle;
		}

		// Clamp angles to avoid extreme values
		wheelColliders.frontLeftWheel.steerAngle = Mathf.Clamp(wheelColliders.frontLeftWheel.steerAngle, -dynamicMaxSteeringAngle, dynamicMaxSteeringAngle);
		wheelColliders.frontRightWheel.steerAngle = Mathf.Clamp(wheelColliders.frontRightWheel.steerAngle, -dynamicMaxSteeringAngle, dynamicMaxSteeringAngle);
	}

	private void HandleWheels()
	{
		Quaternion offset = Quaternion.Euler(0, -90, 0);
		UpdateWheelPosition(wheelColliders.frontLeftWheel, wheelMeshRenderers.frontLeftWheel, offset);
		UpdateWheelPosition(wheelColliders.frontRightWheel, wheelMeshRenderers.frontRightWheel, offset);
		UpdateWheelPosition(wheelColliders.rearLeftWheel, wheelMeshRenderers.rearLeftWheel, offset);
		UpdateWheelPosition(wheelColliders.rearRightWheel, wheelMeshRenderers.rearRightWheel, Quaternion.Euler(0, 90, 0));
	}

	private void UpdateWheelPosition(WheelCollider wheelCollider, MeshRenderer wheelMeshRenderer, Quaternion rotationOffset)
	{
		Vector3 position;
		Quaternion rotation;
		wheelCollider.GetWorldPose(out position, out rotation);
		wheelMeshRenderer.transform.position = position;
		wheelMeshRenderer.transform.rotation = rotation * rotationOffset;
	}

	private void HandleBrake()
	{
		float frontWheelBrakeTorque = brakeInput * brakePower * 0.7f;

		wheelColliders.frontLeftWheel.brakeTorque = frontWheelBrakeTorque;
		wheelColliders.frontRightWheel.brakeTorque = frontWheelBrakeTorque;

		float rearWheelBrakeTorque = brakeInput * brakePower * 0.3f;

		wheelColliders.rearLeftWheel.brakeTorque = rearWheelBrakeTorque;
		wheelColliders.rearRightWheel.brakeTorque = rearWheelBrakeTorque;
	}

	private void HandleBrakeDuringSlip()
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

	private void FindUIElements()
	{
		speedText = FindAndAssignComponent("Speed Text", speedText);
		rpmText = FindAndAssignComponent("RPM Text", rpmText);
		gearText = FindAndAssignComponent("Gear Text", gearText);
		rpmNeedle = FindAndAssignComponent("RPM Needle", rpmNeedle);

	}

	public static T FindAndAssignComponent<T>(string name, T existingComponent) where T : Component
	{
		if (existingComponent == null)
		{
			GameObject foundObject = GameObject.Find(name);
			if (foundObject != null)
			{
				foundObject.TryGetComponent(out existingComponent);
			}
		}
		return existingComponent;
	}

	private void HandleClutch()
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

	public float GetCarSpeed()
	{
		return PlayerCarRb.linearVelocity.magnitude;
	}

	public void UpdateCarSpeedRatio()
	{
		float throttle = Mathf.Clamp(Mathf.Abs(throttleInput), 0.5f, 1f);

		carSpeedRatio = currentRpm * throttle / redLine;
	}

	private void UpdateReversing()
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

	public void DisableCar()
	{
		// gear
		currentGear = 0;
		currentRpm = 0;
		clutch = 0;
		gearState = GearState.Neutral;

		isInputEnabled = false;

		// mask other racers from colliding with this car
		// this.GetComponent<BoxCollider>().excludeLayers = LayerMask.GetMask("AI");

		if (!this.gameObject.CompareTag("Player"))
		{
			StartCoroutine(HideCar());
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	public void EnableCar()
	{
		isInputEnabled = true;
		// this.GetComponent<BoxCollider>().excludeLayers = 0;
		gameObject.SetActive(true);
	}

	private IEnumerator ChangeGear(int gearChange, float throttleInput)
	{
		gearState = GearState.CheckingChange;

		if (currentGear + gearChange >= -1f)
		{
			float carSpeedKmh = GetCarSpeed() * 3.6f;
			float[] gearSpeedLimits = { 0f, 90f, 120f, 150f, 190f, 220f, 250f, 285f, 350f };

			if (gearChange > 0)
			{
				yield return new WaitForSeconds(0.7f);

				if (carSpeedKmh < gearSpeedLimits[currentGear] || carSpeedKmh >= gearSpeedLimits[currentGear + 1] || currentGear >= gearRatios.Length - 1)
				{
					gearState = GearState.Running;
					yield break;
				}

				if (currentRpm < increaseGearRpm)
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

				if (currentGear <= 0)
				{
					gearState = GearState.Running;
					yield break;
				}

				if (currentRpm > decreaseGearRpm)
				{
					gearState = GearState.Running;
					yield break;
				}
			}

			// Apply gear change
			gearState = GearState.ChangingGear;
			yield return new WaitForSeconds(changeGearDelay);
			currentGear += gearChange;

			// Trigger gear change event
			OnGearChanged?.Invoke(currentGear);
		}

		if (gearState != GearState.Neutral)
		{
			gearState = GearState.Running;
		}
	}

	private IEnumerator HideCar(float seconds = 5f)
	{
		yield return new WaitForSeconds(seconds);
		this.gameObject.SetActive(false);
	}
}