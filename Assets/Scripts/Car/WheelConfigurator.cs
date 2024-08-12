using System;
using UnityEngine;

public class WheelConfigurator : MonoBehaviour
{
	private CarLocomotionManager carLocomotionManager;

	[SerializeField] private WheelSettings frontWheelSettings;
	[SerializeField] private WheelSettings rearWheelSettings;

	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();
	}

	private void Start()
	{
		ApplyDefaultWheelSettings();
	}

	private void ApplyDefaultWheelSettings()
	{
		ConfigureWheel(carLocomotionManager.wheelColliders.frontLeftWheel, frontWheelSettings);
		ConfigureWheel(carLocomotionManager.wheelColliders.frontRightWheel, frontWheelSettings);
		ConfigureWheel(carLocomotionManager.wheelColliders.rearLeftWheel, rearWheelSettings);
		ConfigureWheel(carLocomotionManager.wheelColliders.rearRightWheel, rearWheelSettings);
	}

	private void ConfigureWheel(WheelCollider wheelCollider, WheelSettings settings)
	{
		wheelCollider.mass = settings.mass;
		wheelCollider.radius = settings.radius;
		wheelCollider.wheelDampingRate = settings.wheelDampingRate;
		wheelCollider.suspensionDistance = settings.suspensionDistance;
		wheelCollider.forceAppPointDistance = settings.forceAppPointDistance;
		wheelCollider.center = settings.center;

		JointSpring suspensionSpring = new JointSpring
		{
			spring = settings.suspensionSpring.spring,
			damper = settings.suspensionSpring.damper,
			targetPosition = settings.suspensionSpring.targetPosition
		};

		wheelCollider.suspensionSpring = suspensionSpring;

		WheelFrictionCurve forwardFriction = new WheelFrictionCurve
		{
			extremumSlip = settings.forwardFriction.extremumSlip,
			extremumValue = settings.forwardFriction.extremumValue,
			asymptoteSlip = settings.forwardFriction.asymptoteSlip,
			asymptoteValue = settings.forwardFriction.asymptoteValue,
			stiffness = settings.forwardFriction.stiffness
		};

		wheelCollider.forwardFriction = forwardFriction;

		WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve
		{
			extremumSlip = settings.sidewaysFriction.extremumSlip,
			extremumValue = settings.sidewaysFriction.extremumValue,
			asymptoteSlip = settings.sidewaysFriction.asymptoteSlip,
			asymptoteValue = settings.sidewaysFriction.asymptoteValue,
			stiffness = settings.sidewaysFriction.stiffness
		};

		wheelCollider.sidewaysFriction = sidewaysFriction;
	}
}

[Serializable]
public class WheelSettings
{
	public float mass;
	public float radius;
	public float wheelDampingRate;
	public float suspensionDistance;
	public float forceAppPointDistance;
	public Vector3 center;
	public SuspensionSpring suspensionSpring;
	public ForwardFriction forwardFriction;
	public SidewaysFriction sidewaysFriction;
}

[Serializable]
public class SuspensionSpring
{
	public float spring;
	public float damper;
	public float targetPosition;
}

[Serializable]
public class ForwardFriction
{
	public float extremumSlip;
	public float extremumValue;
	public float asymptoteSlip;
	public float asymptoteValue;
	public float stiffness;
}

[Serializable]
public class SidewaysFriction
{
	public float extremumSlip;
	public float extremumValue;
	public float asymptoteSlip;
	public float asymptoteValue;
	public float stiffness;
}


