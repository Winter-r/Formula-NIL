using UnityEngine;
using UnityEngine.UI;

public class CircuitSelectionUI : MonoBehaviour
{
	[SerializeField] private Button fujiSpeedwayButton;
	[SerializeField] private Button playgroundButton;

	private void Awake()
	{
		fujiSpeedwayButton.onClick.AddListener(() =>
		{
			// Load the Fuji Speedway scene
			UnityEngine.SceneManagement.SceneManager.LoadScene("FujiSpeedway");
		});

		playgroundButton.onClick.AddListener(() =>
		{
			// Load the Playground scene
			UnityEngine.SceneManagement.SceneManager.LoadScene("ExampleCircuit");
		});
	}
}
