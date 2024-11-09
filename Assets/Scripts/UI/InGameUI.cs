using UnityEngine;

public class InGameUI : MonoBehaviour
{
	[SerializeField] private GameObject lapTimeUI;
	[SerializeField] private GameObject guageClusterUI;

	private void Start()
	{
		lapTimeUI.SetActive(true);
		guageClusterUI.SetActive(true);
	}

	private void Update()
	{
		if (CarInputManager.uiInput)
		{
			lapTimeUI.SetActive(false);
			guageClusterUI.SetActive(false);
		}
		else
		{
			lapTimeUI.SetActive(true);
			guageClusterUI.SetActive(true);
		}
	}
}