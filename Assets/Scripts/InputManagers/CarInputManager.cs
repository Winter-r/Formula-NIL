using UnityEngine;

public class CarInputManager : MonoBehaviour
{
	private static InputActions CarInput;

	private CarLocomotionManager carLocomotionManager;

	public static float throttleInput;
	public static float dampenedThrottleInput;

	public static float steerInput;
	public static float dampenedSteeringInput;

	public static float clutchInput;
	public static float dampenedClutchInput;

	public static bool cameraCycleInput;

	public static bool pauseInput = false;

	public static float dampeningSpeed = 5f;

	private void OnEnable()
	{
		if (CarInput == null)
		{
			CarInput = new InputActions();

			CarInput.Controls.ThrottleBrake.performed += ctx => throttleInput = ctx.ReadValue<float>();
			CarInput.Controls.ThrottleBrake.canceled += ctx => throttleInput = 0;

			CarInput.Controls.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
			CarInput.Controls.Steer.canceled += ctx => steerInput = 0;

			CarInput.Controls.Clutch.performed += ctx => clutchInput = ctx.ReadValue<float>();
			CarInput.Controls.Clutch.canceled += ctx => clutchInput = 0;

			CarInput.Controls.CycleCamera.performed += ctx => cameraCycleInput = ctx.ReadValueAsButton();
			CarInput.Controls.CycleCamera.canceled += ctx => cameraCycleInput = false;

			CarInput.Controls.Pause.performed += ctx => TogglePause();
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

		if (carLocomotionManager.isInputEnabled) carLocomotionManager.HandleCarLocomotion(dampenedThrottleInput, dampenedSteeringInput, dampenedClutchInput);
	}

	public static float DampenedInput(float input, float output)
	{
		return Mathf.Lerp(output, input, dampeningSpeed * Time.deltaTime);
	}

	private void TogglePause()
	{
		pauseInput = !pauseInput;
	}
}
