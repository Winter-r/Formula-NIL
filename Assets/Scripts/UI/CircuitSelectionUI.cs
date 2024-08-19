using UnityEngine;
using UnityEngine.UI;

public class CircuitSelectionUI : MonoBehaviour
{
	[SerializeField] private Button fujiSpeedwayButton;
	[SerializeField] private Button bahrainCircuitButton;

	private void Awake()
	{
		fujiSpeedwayButton.onClick.AddListener(() =>
		{
			// Load the Fuji Speedway scene
			UnityEngine.SceneManagement.SceneManager.LoadScene("FujiSpeedway");
		});

		bahrainCircuitButton.onClick.AddListener(() =>
		{
			// Load the Bahrain Circuit scene
			UnityEngine.SceneManagement.SceneManager.LoadScene("BahrainCircuit");
		});
	}
}
