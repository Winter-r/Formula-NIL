using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UINavigationInputManager : MonoBehaviour
{
	public static InputActions navigationInput;

	private void OnEnable()
	{
		if (EventSystem.current.firstSelectedGameObject == null)
		{
			return;
		}

		if (navigationInput == null)
		{
			navigationInput = new InputActions();

			navigationInput.Navigation.Mouse.performed += ctx => DeselectCurrentButton();
			navigationInput.Navigation.Keyboard.performed += ctx => ReselectButton();
		}

		navigationInput.Enable();

		// Subscribe to scene loaded event
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		navigationInput.Disable();

		// Unsubscribe from scene loaded event
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void DeselectCurrentButton()
	{
		Debug.Log("DeselectCurrentButton called");
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			Debug.Log("Button deselected");
		}
	}

	private void ReselectButton()
	{
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
			Debug.Log("Button reselected");
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Re-enable input actions after loading a new scene
		navigationInput.Enable();
	}
}
