using UnityEngine;

public class GarageInputManager : MonoBehaviour
{
	public static InputActions GarageInput;

	public static float cycleCarColorInput;

	private void OnEnable()
	{
		if (GarageInput == null)
		{
			GarageInput = new InputActions();

			GarageInput.Garage.Back.performed += ctx => GarageUI.GoBack();

			GarageInput.Garage.Cycle.performed += ctx => cycleCarColorInput = ctx.ReadValue<float>();
			GarageInput.Garage.Cycle.canceled += ctx => cycleCarColorInput = 0;
		}

		GarageInput.Enable();
	}

	private void OnDisable()
	{
		GarageInput.Disable();
	}
}
