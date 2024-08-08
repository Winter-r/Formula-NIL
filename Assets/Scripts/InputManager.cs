using UnityEngine;

public class InputManager : MonoBehaviour
{
	private static CarInput CarInput;

	// public static bool isDrifting;
	private CarLocomotionManager carLocomotionManager;

	public static float throttleInput;
	public static float dampenedThrottleInput;

	public static float steerInput;
	public static float dampenedSteeringInput;

	public static float clutchInput;
	public static float dampenedClutchInput;

	// public static float handBrakeInput;

	public static bool cameraCycleInput;

	public static float dampeningSpeed = 5f;

	private void OnEnable()
	{
		if (CarInput == null)
		{
			CarInput = new CarInput();

			CarInput.Controls.ThrottleBrake.performed += ctx => throttleInput = ctx.ReadValue<float>();
			CarInput.Controls.ThrottleBrake.canceled += ctx => throttleInput = 0;

			CarInput.Controls.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
			CarInput.Controls.Steer.canceled += ctx => steerInput = 0;

			CarInput.Controls.Clutch.performed += ctx => clutchInput = ctx.ReadValue<float>();
			CarInput.Controls.Clutch.canceled += ctx => clutchInput = 0;

			// CarInput.Controls.HandBrake.performed += ctx => handBrakeInput = ctx.ReadValue<float>();
			// CarInput.Controls.HandBrake.canceled += ctx => handBrakeInput = 0;

			CarInput.Controls.CycleCamera.performed += ctx => cameraCycleInput = ctx.ReadValueAsButton();
			CarInput.Controls.CycleCamera.canceled += ctx => cameraCycleInput = false;
		}

		CarInput.Enable();
	}

	private void OnDisable()
	{
		CarInput.Disable();
	}

	private void Awake()
	{
		carLocomotionManager = GetComponent<CarLocomotionManager>();
	}

	private void Update()
	{
		dampenedThrottleInput = DampenedInput(throttleInput, dampenedThrottleInput);
		dampenedSteeringInput = DampenedInput(steerInput, dampenedSteeringInput);
		dampenedClutchInput = DampenedInput(clutchInput, dampenedClutchInput);

		carLocomotionManager.HandleCarLocomotion(dampenedThrottleInput, dampenedSteeringInput, dampenedClutchInput, 0);

		Debug.Log("Throttle: " + throttleInput + " | Clutch: " + clutchInput);
	}

	public static float DampenedInput(float input, float output)
	{
		return Mathf.Lerp(output, input, dampeningSpeed * Time.deltaTime);
	}
}
