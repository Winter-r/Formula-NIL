using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircuitSelectionUI : MonoBehaviour
{
	[SerializeField] private Button backButton;
	[SerializeField] private Button bahrainCircuitButton;

	private void Awake()
	{
		// Add a listener to the back button
		backButton.onClick.AddListener(() =>
		{
			// Load the main menu scene
			SceneManager.LoadScene("MainMenu");
		});

		bahrainCircuitButton.onClick.AddListener(() =>
		{
			// Load the Bahrain Circuit scene
			SceneManager.LoadScene("BahrainCircuit");
		});
	}
}
