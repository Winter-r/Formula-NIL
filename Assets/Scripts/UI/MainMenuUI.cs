using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
	[SerializeField] private Button _playButton;
	[SerializeField] private Button _garageButton;
	[SerializeField] private Button _leaderboardButton;
	[SerializeField] private Button _optionsButton;
	[SerializeField] private Button _exitButton;
	
	private void Awake()
	{
		_playButton.onClick.AddListener(() =>
		{
			// Load the game scene
			SceneManager.LoadScene("CircuitSelection");
		});
		
		_garageButton.onClick.AddListener(() =>
		{
			// Load the garage scene
			SceneManager.LoadScene("Garage");
		});
		
		_leaderboardButton.onClick.AddListener(() =>
		{
			// Load the leaderboard scene
			SceneManager.LoadScene("Leaderboard");
		});
		
		_optionsButton.onClick.AddListener(() =>
		{
			// Load the options scene
			SceneManager.LoadScene("Options");
		});
		
		_exitButton.onClick.AddListener(() =>
		{
			// Quit the game
			Application.Quit();
		});
	}
}
