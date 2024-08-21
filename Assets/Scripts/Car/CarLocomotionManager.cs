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
	private const float TORQUE_MULTIPLIER = 5252f;

	#region Variables

	#region General Settings

	public Rigidbody PlayerCarRb { get; private set; }
	public Transform PlayerCar { get; private set; }

	[HideInInspector] public bool isReversing;

	public event Action<int> OnGearChanged;

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
	[SerializeField] private float maxSteeringAngle;
	private float ackermannAngleLeft;
	private float ackermannAngleRight;

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
	[SerializeField] private float rpmSpeedFactor = 5f;
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
		HandleMotor(throttleInput);
		HandleSteering(steerInput);
		HandleBrake();
		HandleBrakeDuringSlip(throttleInput);
		HandleStartingEngine(throttleInput);
		HandleClutch(throttleInput, clutchInput);
		HandleWheels();
		UpdateReversing(throttleInput);
		UpdateCarSpeedRatio(throttleInput);
		UpdateGaugeClusterUI();

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
		if (RaceManager.Instance.raceType == RaceType.TimeTrial && !RaceManager.Instance.TrialStarted)
		{
			// If race hasn't started, skip applying motor torque but allow RPM to increase with clutch
			currentTorque = CalculateTorque(throttleInput);
			return;
		}
		else if (RaceManager.Instance.raceType == RaceType.AIGrandPrix && !RaceManager.Instance.RaceStarted)
		{
			currentTorque = CalculateTorque(throttleInput);
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
				currentRpm = Mathf.Lerp(currentRpm, Mathf.Max(idleRpm, redLine * throttleInput) + UnityEngine.Random.Range(-50, 50), Time.deltaTime);
			}
			else
			{
				wheelRpm = Mathf.Abs((wheelColliders.rearRightWheel.rpm + wheelColliders.rearLeftWheel.rpm) / 2f) * (isReversing ? reverseGearRatio : gearRatios[currentGear]) * differentialRatio;
				currentRpm = Mathf.Lerp(currentRpm, Mathf.Max(idleRpm - 100, wheelRpm), Time.deltaTime * rpmSpeedFactor);

				if (isReversing)
				{
					torque = (powerCurve.Evaluate(currentRpm / redLine) * motorPower / currentRpm) * reverseGearRatio * differentialRatio * TORQUE_MULTIPLIER * clutch;
				}
				else
				{
					torque = (powerCurve.Evaluate(currentRpm / redLine) * motorPower / currentRpm) * gearRatios[currentGear] * differentialRatio * TORQUE_MULTIPLIER * clutch;
				}
			}
		}

		return torque;
	}

	private void HandleSteering(float steerInput)
	{
		float carSpeed = GetCarSpeed();
		float speedFactor = Mathf.Clamp01(carSpeed / maxSpeed);
		float adjustedSteerInput = steerInput * Mathf.Lerp(0.5f, 0.05f, speedFactor);

		if (adjustedSteerInput > 0)
		{
			ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * adjustedSteerInput;
			ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * adjustedSteerInput;
		}
		else if (adjustedSteerInput < 0)
		{
			ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * adjustedSteerInput;
			ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * adjustedSteerInput;
		}
		else
		{
			ackermannAngleLeft = 0;
			ackermannAngleRight = 0;
		}

		ackermannAngleLeft = Mathf.Clamp(ackermannAngleLeft, -maxSteeringAngle, maxSteeringAngle);
		ackermannAngleRight = Mathf.Clamp(ackermannAngleRight, -maxSteeringAngle, maxSteeringAngle);

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

	private void HandleBrake()
	{
		float frontWheelBrakeTorque = brakeInput * brakePower * 0.7f;

		wheelColliders.frontLeftWheel.brakeTorque = frontWheelBrakeTorque;
		wheelColliders.frontRightWheel.brakeTorque = frontWheelBrakeTorque;

		float rearWheelBrakeTorque = brakeInput * brakePower * 0.3f;

		wheelColliders.rearLeftWheel.brakeTorque = rearWheelBrakeTorque;
		wheelColliders.rearRightWheel.brakeTorque = rearWheelBrakeTorque;
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

	public void ResetCar()
	{
		PlayerCarRb.linearVelocity = Vector3.zero;
		PlayerCarRb.angularVelocity = Vector3.zero;

		gearState = GearState.Neutral;
		currentGear = 0;
		currentRpm = 0;
		clutch = 0;

		this.gameObject.SetActive(false);
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

			// Fire the event after the gear change
			OnGearChanged?.Invoke(currentGear);
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

[Serializable]
public class CameraView
{
	public ViewType viewType;
	public Transform viewTransform;
}

public enum ViewType
{
	Regular,
	Far,
	Pod
}