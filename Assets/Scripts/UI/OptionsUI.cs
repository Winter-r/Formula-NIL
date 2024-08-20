using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
	[SerializeField] private ConfirmationPanel confirmationPanel;
	[SerializeField] private Button resetPlayerPrefsButton;
	[SerializeField] private Button backButton;

	private void Start()
	{
		confirmationPanel.HideConfirmationPanel();

		backButton.onClick.AddListener(() =>
		{
			SceneManager.LoadScene("MainMenu");
		});

		resetPlayerPrefsButton.onClick.AddListener(() =>
		{
			confirmationPanel.ShowConfirmationPanel();
		});

		confirmationPanel.yesButton.onClick.AddListener(() =>
		{
			PlayerPrefs.DeleteAll();
			confirmationPanel.HideConfirmationPanel();
		});

		confirmationPanel.noButton.onClick.AddListener(() =>
		{
			confirmationPanel.HideConfirmationPanel();
		});
	}
}

[Serializable]
public class ConfirmationPanel
{
	public GameObject graphic;
	public Button yesButton;
	public Button noButton;

	public void ShowConfirmationPanel()
	{
		graphic.SetActive(true);
	}

	public void HideConfirmationPanel()
	{
		graphic.SetActive(false);
	}
}