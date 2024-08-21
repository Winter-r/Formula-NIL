using UnityEngine;
using UnityEngine.UI;

public class GarageUI : MonoBehaviour
{
	public Button backButton;
	public Button cycleCarColorUpButton;
	public Button cycleCarColorDownButton;

	public Material playerMaterial;

	private void OnEnable()
	{
		backButton.onClick.AddListener(() =>
		{
			GoBack();
		});

		cycleCarColorUpButton.onClick.AddListener(() =>
		{
			CycleCarColor(1);
		});

		cycleCarColorDownButton.onClick.AddListener(() =>
		{
			CycleCarColor(-1);
		});

		// Load the player preference for the car color
		playerMaterial.mainTextureOffset = new Vector2(PlayerPrefs.GetFloat("CarColorOffsetX"), 0.5f);
	}

	private void Update()
	{
		if (GarageInputManager.cycleCarColorInput > 0)
		{
			CycleCarColor(1);
		}
		else if (GarageInputManager.cycleCarColorInput < 0)
		{
			CycleCarColor(-1);
		}
	}

	public static void GoBack()
	{
		// Load the Main Menu scene
		UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}

	// params for positive or negative
	public void CycleCarColor(int direction)
	{		// Cycle through the car colors, by adjusting the X-offset of the main texture by 0.1 and keeping the Y-offset at 0.5
		playerMaterial.mainTextureOffset += new Vector2(0.1f * direction, 0);
		PlayerPrefs.SetFloat("CarColorOffsetX", playerMaterial.mainTextureOffset.x);
	}
}
