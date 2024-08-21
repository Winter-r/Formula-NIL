using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsInputManager : MonoBehaviour
{
	public static InputActions optionsInput;

	private void OnEnable()
	{
		if (optionsInput == null)
		{
			optionsInput = new InputActions();

			optionsInput.CircuitSelection.Back.performed += ctx => SceneManager.LoadScene("MainMenu");
		}

		optionsInput.Enable();
	}

	private void OnDisable()
	{
		optionsInput.Disable();
	}
}