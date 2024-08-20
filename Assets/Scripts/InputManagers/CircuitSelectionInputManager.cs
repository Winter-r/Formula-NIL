using UnityEngine;
using UnityEngine.SceneManagement;

public class CircuitSelectionInputManager : MonoBehaviour
{
	public static InputActions circuitSelectionInput;

	private void OnEnable()
	{
		if (circuitSelectionInput == null)
		{
			circuitSelectionInput = new InputActions();

			circuitSelectionInput.CircuitSelection.Back.performed += ctx => SceneManager.LoadScene("MainMenu");
		}

		circuitSelectionInput.Enable();
	}

	private void OnDisable()
	{
		circuitSelectionInput.Disable();
	}
}