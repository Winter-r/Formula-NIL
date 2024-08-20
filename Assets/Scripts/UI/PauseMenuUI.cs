using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
	[SerializeField] private GameObject graphic;
	[SerializeField] private Button resumeButton;
	[SerializeField] private Button quitButton;

	private void Awake()
	{
		graphic.SetActive(false);

		resumeButton.onClick.AddListener(() =>
		{
			Time.timeScale = 1;
			graphic.SetActive(false);
			CarInputManager.pauseInput = false;
		});

		quitButton.onClick.AddListener(() =>
		{
			Time.timeScale = 1;
			graphic.SetActive(false);
			CarInputManager.pauseInput = false;
			SceneManager.LoadScene("MainMenu");
		});
	}

	private void Update()
	{
		if (CarInputManager.pauseInput)
		{
			if (Time.timeScale == 1)
			{
				Time.timeScale = 0;
				graphic.SetActive(true);
			}
		}
		else
		{
			if (Time.timeScale == 0)
			{
				Time.timeScale = 1;
				graphic.SetActive(false);
			}
		}
	}
}
